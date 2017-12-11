using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace ME7correction
{
    public partial class frmMain : Form
    {
        public string filePath = "";
        public string correctedFilePath = "";
        public string binName = "";
        public string ecuPN = "";
        public string ecuSW = "";
        public string engine = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        public frmMain()
        {
            InitializeComponent();
            lblVersion.Text = "v01.00.01";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lblCheckStatus.ForeColor = Color.White;
            lblCheckStatus.Text = "N/A";
            lblCorrectStatus.ForeColor = Color.White;
            lblCorrectStatus.Text = "N/A";
            lblCorrectedCheckStatus.ForeColor = Color.White;
            lblCorrectedCheckStatus.Text = "N/A";
            txtOutput.Clear();
            txtENG.Clear();
            txtPN.Clear();
            txtSW.Clear();
            engine = "";
            ecuPN = "";
            ecuSW = "";
            binName = "";
            filePath = "";
            lblFileName.Text = "";

            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Bin Files (.bin)|*.bin";
            openFileDialog1.FilterIndex = 1;

            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK) // Test result.
                {
                    filePath = openFileDialog1.FileName;

                    int amount = filePath.Length;
                    int x = 1;
                    string tChar = filePath.Substring(amount - x, 1);

                    while(tChar != "\\")
                    {
                        x++;
                        binName = tChar + binName;
                        tChar = filePath.Substring(amount - x, 1);
                    }
                    
                    lblFileName.Text = binName;

                    btnCheck.Enabled = true;
                    txtOutput.Text = "";
                    txtOutput.AppendText("\"" + binName + "\" has been selected.\n");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error selecting that .bin file: " + ex.Message);
            }

        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            lblCheckStatus.ForeColor = Color.White;
            lblCheckStatus.Text = "N/A";
            lblCorrectStatus.ForeColor = Color.White;
            lblCorrectStatus.Text = "N/A";
            lblCorrectedCheckStatus.ForeColor = Color.White;
            lblCorrectedCheckStatus.Text = "N/A";
            txtOutput.Clear();
            txtENG.Clear();
            txtPN.Clear();
            txtSW.Clear();
            engine = "";
            ecuPN = "";
            ecuSW = "";

            int value = 5;
            value = me7Check(filePath);

            if(value == 1)
            {
                txtENG.Clear();
                txtPN.Clear();
                txtSW.Clear();
                txtENG.Text = engine;
                txtPN.Text = ecuPN;
                txtSW.Text = ecuSW;

                lblCheckStatus.ForeColor = Color.Red;
                lblCheckStatus.Text = "FAIL";

                DialogResult result = MessageBox.Show("Errors have been found. Would you like to try correcting the checksum?","Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    value = me7Sum(filePath);
                    if(value == 1)
                    {
                        lblCorrectStatus.ForeColor = Color.Green;
                        lblCorrectStatus.Text = "PASS";

                        value = me7Check(correctedFilePath);
                        if(value == 1)
                        {
                            lblCorrectedCheckStatus.ForeColor = Color.Green;
                            lblCorrectedCheckStatus.Text = "PASS";
                            txtOutput.Text += "\n\r\n\r" + "Your corrected .bin file is located at: \n\r" + correctedFilePath;
                            MessageBox.Show("The checksum has been corrected and the new .bin file has been validated.", "PASS");
                        }
                        else if(value == 0)
                        {
                            
                            lblCorrectedCheckStatus.ForeColor = Color.Red;
                            lblCorrectedCheckStatus.Text = "FAIL";
                            MessageBox.Show("There were some errors after checking the newly corrected .bin file.", "FAIL");
                        }
                    }
                    else if(value == 1)
                    {
                        lblCorrectStatus.ForeColor = Color.Red;
                        lblCorrectStatus.Text = "FAIL";
                        MessageBox.Show("There were some errors when trying to correct the .bin file.", "FAIL");
                    }
                }
            }
            else if (value == 0)
            {
                txtENG.Clear();
                txtPN.Clear();
                txtSW.Clear();
                txtENG.Text = engine;
                txtPN.Text = ecuPN;
                txtSW.Text = ecuSW;

                lblCheckStatus.ForeColor = Color.Green;
                lblCheckStatus.Text = "PASS";
            }
        }

        public int me7Check(string myFilePath)
        {
            string line = "";

            System.Diagnostics.Process proc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\me7check.exe",
                    Arguments = "\"" + myFilePath + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            try
            {
                proc.Start();

                while (!proc.StandardOutput.EndOfStream)
                {
                    line = proc.StandardOutput.ReadLine();
                    checkLine(line);
                    txtOutput.Text += "\r\n" + line;
                }
                txtOutput.Text += "\r\n\r\n";

                btnLog.Enabled = true;

                if (txtOutput.Text.Contains("errors!!!"))
                {
                    return 1;
                }
                else if (txtOutput.Text.Contains("File is OK"))
                {
                    return 0;
                }

                MessageBox.Show("ME7check - An error has occured: \nThe selected .bin file is not a valid ME7.x .bin dump. Please verify file and filepath");
                return 2;
            }
            catch(Exception ex)
            {
                MessageBox.Show("ME7check - An error has occured: \n" + ex.Message);
                return 2;
            }
        }

        public int me7Sum(string myFilePath)
        {
            string line = "";
            int length = myFilePath.Length;
            // Create an instance of the open file dialog box.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            
            // Set filter options and filter index.
            saveFileDialog1.Filter = "Binary Files|*.bin";
            saveFileDialog1.FilterIndex = 1;

            try
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK) // Test result.
                {
                    correctedFilePath = saveFileDialog1.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error selecting that filepath: " + ex.Message);
            }

            //correctedFilePath = myFilePath.Substring(0, length - 4) + "_corrected.bin";

            System.Diagnostics.Process proc = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = Environment.CurrentDirectory + "\\me7sum.exe",
                    Arguments = "\"" + myFilePath + "\" " + "\"" + correctedFilePath + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            try
            {
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    line = proc.StandardOutput.ReadLine();
                    txtOutput.Text += "\r\n" + line;
                }
                txtOutput.Text += "\r\n\r\n";

                if (!txtOutput.Text.Contains("DONE!"))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("ME7sum - An error has occured: \n" + ex.Message);
                return 2;
            }
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            txtOutput.SelectionStart = txtOutput.Text.Length;
            txtOutput.ScrollToCaret();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            btnCheck.Enabled = false;
            btnLog.Enabled = false;
            txtOutput.Clear();
            lblFileName.Text = "";
            correctedFilePath = "";
            filePath = "";
            binName = "";
            engine = "";
            ecuSW = "";
            ecuPN = "";
            txtENG.Clear();
            txtPN.Clear();
            txtSW.Clear();
            lblCheckStatus.ForeColor = Color.White;
            lblCheckStatus.Text = "N/A";
            lblCorrectStatus.ForeColor = Color.White;
            lblCorrectStatus.Text = "N/A";
            lblCorrectedCheckStatus.ForeColor = Color.White;
            lblCorrectedCheckStatus.Text = "N/A";

        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            try
            {
                int length = filePath.Length;
                string fileName = filePath.Substring(0, length - 4) + "_log.txt";
                //creating the lotto file
                FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                fs.Close();

                StreamWriter sw = new StreamWriter(fileName);
                sw.Write(txtOutput.Text);
                sw.Close();
                txtOutput.Text += "\n\r\n\r Log file created successfully: " + fileName;
                MessageBox.Show("Log file created successfully:\n" + fileName);

            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error while creating the log file: \n" + ex.Message);
            }
        }

        private void checkLine(string inputLine)
        {
            string tChar = "";
            int x = 0;

            if (inputLine.Contains("SSECUSN"))
            {
                ecuSW = inputLine.Substring(10, 6);
            }
            else if (inputLine.Contains("VAG part number"))
            {
                x = 6;
                tChar = inputLine.Substring(x, 1);
                while (tChar != "'")
                {
                    ecuPN += tChar;
                    x++;
                    tChar = inputLine.Substring(x, 1);
                }

            }
            else if (inputLine.Contains("engine id"))
            {
                x = 6;
                tChar = inputLine.Substring(x, 1);
                while (tChar != "'")
                {
                    engine += tChar;
                    x++;
                    tChar = inputLine.Substring(x, 1);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            MessageBox.Show("ME7 Correction Utility - GUI created by Murph @Nefarious Motorsports\nAll credit for the \"me7check.exe\" and \"me7sum.exe\" goes to nyet and 360trev.\n\n*** Neither Nefarious Motorsports, nyet, 360trev, or I the creator of this utility take responsibility for the failed validation or correction of any .bin files.\nNor take responsiblity for failed flashing or bricking of an ECU due to an incorrect or faulty .bin file used.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
