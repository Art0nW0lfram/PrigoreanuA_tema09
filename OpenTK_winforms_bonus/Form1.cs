using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_winforms_z02
{
    public partial class Form1 : Form
    {
        // --- CONSTANTE ȘI VARIABILE PENTRU SLOT MACHINE ---
        private int[] slotTextureIds = new int[4]; // ID-urile pentru cele 4 imagini (simboluri)
        private int[] currentSlotState = { 0, 0, 0 }; // Ce imagine afișează fiecare slot (0-3)

        // Control animație
        private Timer slotTimer = new Timer();
        private int cyclesRemaining = 0;
        private Random rng = new Random();

        // Controale UI (le generăm din cod ca să nu depindem de Designer)
        private Button btnSpin;
        private Label lblBigWin; // Label-ul pentru mesajul WINNER
        private NumericUpDown nudCycles;
        private Label lblStatus;
        private Label lblCredits;

        // Creditul jucătorului (ca în imagine)
        private int playerCredits = 100;

        public Form1()
        {
            InitializeComponent();
            // Configurare fereastră
            this.Size = new Size(800, 600);
            this.Text = "EGC Slot Machine Bonus";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Setări OpenGL
            GL.ClearColor(Color.FromArgb(20, 20, 50)); // Fundal albastru închis (stil casino)
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Setup Interfață și Texturi
            SetupInterface();
            // --- SETUP BIG WIN LABEL ---
            lblBigWin = new Label();
            lblBigWin.Text = "WINNER!";
            lblBigWin.Font = new Font("Impact", 72, FontStyle.Bold); // Font foarte mare
            lblBigWin.ForeColor = Color.Yellow; // Text Galben
            lblBigWin.BackColor = Color.Red;    // Fundal Roșu (să iasă în evidență)
            lblBigWin.AutoSize = true;
            lblBigWin.Visible = false;          // Ascuns la început
            this.Controls.Add(lblBigWin);
            lblBigWin.BringToFront();           // Să fie peste OpenGL
            LoadSlotTextures();

            // Setăm o vedere 2D (Ortografică) pentru că sloturile sunt 2D
            SetupViewport();
        }

        private void SetupViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            // Setăm sistemul de coordonate: 0,0 în centru.
            GL.Ortho(-400, 400, -300, 300, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void SetupInterface()
        {
            // 1. Timer
            slotTimer.Interval = 100; // Viteză animație (0.1s pentru fluiditate)
            slotTimer.Tick += SlotTimer_Tick;

            // 2. Buton SPIN (Stil buton verde mare din imagine)
            btnSpin = new Button();
            btnSpin.Text = "SPIN";
            btnSpin.Font = new Font("Arial", 14, FontStyle.Bold);
            btnSpin.BackColor = Color.LimeGreen;
            btnSpin.ForeColor = Color.White;
            btnSpin.Size = new Size(100, 60);
            btnSpin.Location = new Point(650, 480); // Dreapta jos
            btnSpin.Click += BtnSpin_Click;
            this.Controls.Add(btnSpin);
            btnSpin.BringToFront();

            // 3. Selector Cicluri
            Label lblCyc = new Label();
            lblCyc.Text = "Cicluri:";
            lblCyc.ForeColor = Color.White;
            lblCyc.BackColor = Color.Transparent;
            lblCyc.Location = new Point(20, 500);
            this.Controls.Add(lblCyc);

            nudCycles = new NumericUpDown();
            nudCycles.Minimum = 5;
            nudCycles.Maximum = 50;
            nudCycles.Value = 20;
            nudCycles.Location = new Point(70, 500);
            this.Controls.Add(nudCycles);

            // 4. Afișaj Status (Win/Lose)
            lblStatus = new Label();
            lblStatus.Text = "BINE AI VENIT!";
            lblStatus.Font = new Font("Arial", 16, FontStyle.Bold);
            lblStatus.ForeColor = Color.Gold;
            lblStatus.BackColor = Color.Transparent;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(300, 50);
            this.Controls.Add(lblStatus);

            // 5. Credite
            lblCredits = new Label();
            lblCredits.Text = "CREDITE: " + playerCredits;
            lblCredits.Font = new Font("Consolas", 14, FontStyle.Bold);
            lblCredits.ForeColor = Color.Cyan;
            lblCredits.BackColor = Color.Black;
            lblCredits.AutoSize = true;
            lblCredits.Location = new Point(20, 20);
            this.Controls.Add(lblCredits);
        }

        private void LoadSlotTextures()
        {
            // Generează ID-uri
            GL.GenTextures(4, slotTextureIds);

            // Numele fișierelor (trebuie să existe în bin/Debug!)
            string[] files = { "slot1.jpg", "slot2.jpg", "slot3.jpg", "slot4.jpg" };

            for (int i = 0; i < 4; i++)
            {
                if (File.Exists(files[i]))
                {
                    LoadSingleTexture(slotTextureIds[i], files[i]);
                }
                else
                {
                    // Fallback: folosim brickTexture dacă lipsește imaginea slot
                    if (File.Exists("brickTexture.jpg"))
                        LoadSingleTexture(slotTextureIds[i], "brickTexture.jpg");
                }
            }
        }

        private void LoadSingleTexture(int id, string path)
        {
            Bitmap bmp = new Bitmap(path);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
        }

        // --- LOGICA JOCULUI ---

        private void BtnSpin_Click(object sender, EventArgs e)
        {
            lblBigWin.Visible = false;

            if (playerCredits < 10)
            {
                lblStatus.Text = "CREDITE INSUFICIENTE!";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            // Plătește miza
            playerCredits -= 10;
            lblCredits.Text = "CREDITE: " + playerCredits;

            // Pornește rotirea
            cyclesRemaining = (int)nudCycles.Value;
            lblStatus.Text = "SE ÎNVÂRTE...";
            lblStatus.ForeColor = Color.White;
            btnSpin.Enabled = false; // Dezactivează butonul
            slotTimer.Start();
        }

        private void SlotTimer_Tick(object sender, EventArgs e)
        {
            // Schimbă imaginile random
            currentSlotState[0] = rng.Next(0, 4);
            currentSlotState[1] = rng.Next(0, 4);
            currentSlotState[2] = rng.Next(0, 4);

            cyclesRemaining--;

            if (cyclesRemaining <= 0)
            {
                // Forțăm toate sloturile să fie 1 (Șeptarul) la finalul rotirii
                //currentSlotState[0] = 1;
                //currentSlotState[1] = 1;
                //currentSlotState[2] = 1;
                // -----------------------------------------------

                slotTimer.Stop();
                CheckWin();
                btnSpin.Enabled = true;
            }

            GlControl1.Invalidate(); // Redesenează
        }

        private void CheckWin()
        {
            // Condiție victorie: Toate 3 la fel
            if (currentSlotState[0] == currentSlotState[1] && currentSlotState[1] == currentSlotState[2])
            {
                int castig = 100;
                playerCredits += castig;

                // --- LOGICA PENTRU JACKPOT 777 ---
                // Verificăm dacă simbolul câștigător este Șeptarul (indexul 1 - adică a doua imagine încărcată)
                if (currentSlotState[0] == 1)
                {
                    lblBigWin.Text = "WINNER!"; // Setăm textul

                    // Centram textul pe ecran (calcul matematic simplu)
                    lblBigWin.Location = new Point(
                        (this.Width - lblBigWin.Width) / 2,
                        (this.Height - lblBigWin.Height) / 2
                    );

                    lblBigWin.Visible = true; // ÎL AFIȘĂM!
                    lblStatus.Text = "777 JACKPOT!!!";
                }
                else
                {
                    // Câștig normal (ex: 3 cireșe)
                    lblStatus.Text = "JACKPOT! AI CÂȘTIGAT " + castig + "!";
                }

                lblStatus.ForeColor = Color.Gold;
            }
            // Condiție secundară: 2 la fel (mic premiu de consolare)
            else if (currentSlotState[0] == currentSlotState[1] ||
                     currentSlotState[1] == currentSlotState[2] ||
                     currentSlotState[0] == currentSlotState[2])
            {
                int castig = 20;
                playerCredits += castig;
                lblStatus.Text = "MINI-CÂȘTIG! (" + castig + ")";
                lblStatus.ForeColor = Color.Yellow;
            }
            else
            {
                lblStatus.Text = "AI PIERDUT.";
                lblStatus.ForeColor = Color.Gray;
            }

            lblCredits.Text = "CREDITE: " + playerCredits;
        }

        // --- RANDAREA ---

        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Desenează cele 3 sloturi
            // Slot 1 (Stânga)
            DrawSlot(-220, currentSlotState[0]);
            // Slot 2 (Centru)
            DrawSlot(0, currentSlotState[1]);
            // Slot 3 (Dreapta)
            DrawSlot(220, currentSlotState[2]);

            // Desenează chenare (opțional, pentru aspect)
            DrawBorder(-220);
            DrawBorder(0);
            DrawBorder(220);

            GlControl1.SwapBuffers();
        }

        private void DrawSlot(float xPos, int textureIndex)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, slotTextureIds[textureIndex]);
            GL.Color3(Color.White);

            GL.PushMatrix();
            GL.Translate(xPos, 0, 0); // Mută la poziția slotului

            float size = 100.0f; // Mărimea imaginii (jumătate de latură)

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1); GL.Vertex2(-size, -size);
            GL.TexCoord2(1, 1); GL.Vertex2(size, -size);
            GL.TexCoord2(1, 0); GL.Vertex2(size, size);
            GL.TexCoord2(0, 0); GL.Vertex2(-size, size);
            GL.End();

            GL.PopMatrix();
        }

        private void DrawBorder(float xPos)
        {
            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(5.0f);
            GL.Color3(Color.Silver); // Chenar argintiu

            GL.PushMatrix();
            GL.Translate(xPos, 0, 0);

            float size = 105.0f; // Puțin mai mare decât imaginea

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(-size, -size);
            GL.Vertex2(size, -size);
            GL.Vertex2(size, size);
            GL.Vertex2(-size, size);
            GL.End();

            GL.PopMatrix();
        }

        private void GlControl1_Resize(object sender, EventArgs e)
        {
            SetupViewport();
        }

        // Evenimente generate automat care nu ne trebuie, dar trebuie să existe ca să nu dea eroare
        private void GlControl1_Load(object sender, EventArgs e) { }
        private void GlControl1_MouseMove(object sender, MouseEventArgs e) { }
        private void GlControl1_MouseDown(object sender, MouseEventArgs e) { }
        private void GlControl1_MouseUp(object sender, MouseEventArgs e) { }
    }
}