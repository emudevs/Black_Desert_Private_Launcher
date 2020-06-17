using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Xsl;
using System.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;
using System.Xml.Linq;
using System.Text.RegularExpressions;


namespace Black_Desert_Private_Launcher
{
    public partial class Form1 : Form
    {
        bool emailIsEmail = false; // used later to check if username is a valid email or not
        public Form1()
        {
           /*******************************************************************
            * Initialize the form, check if gameclient exists and get ready to go
            *******************************************************************/
            InitializeComponent();                                                   
            this.Text = "Black Desert Online Private Launcher!";                    // Set Form's name
            string currentdir = System.IO.Directory.GetCurrentDirectory();          // get the directory the launcher is run from
            string exeFile = currentdir + "\\bin64\\BlackDesert64.exe";             // look for file under the current dir
            bool ready0 = System.IO.File.Exists(exeFile);                           // check if game client exists returning true or false
            if (ready0 == false)
            {                                                                       // If returned false show error meassages and exit
                MessageBox.Show(" Black Desert Online is missing! \n Make sure the launcher is in the same folder as the 'bin64' folder.", "Important! Game files are missing!");
                Environment.Exit(0);
            }                                                                       // if returned true continue setup

            if (Properties.Settings.Default.Email != string.Empty)                  // check settings, if they exist fill in textboxes with previous data
            {
                textBoxUserEmail.Text = Properties.Settings.Default.Email;          // email
                textBox_Password.Text = Properties.Settings.Default.Password;       // password
                textBoxServerIP.Text = Properties.Settings.Default.serverIP;        // server ip
                textBoxServerPort.Text = Properties.Settings.Default.serverPort;    // server port
                checkBoxRememberMe.Checked = true;                                  // tick "remember me"
            } else
            {
                textBoxUserEmail.Text = "Mail@wateva.com";                          // if settings dont exist fill textboxes with the default data
                textBox_Password.Text = "Password";
                textBoxServerIP.Text = "127.0.0.1";
                textBoxServerPort.Text = "8888";
                checkBoxRememberMe.Checked = false;                                 // untick "remember me"
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*******************************************************************
             * Setup and then start the game client
             *******************************************************************/
            if (checkBoxRememberMe.Checked)                                         // check if remember me is checked
            {
                Properties.Settings.Default.Email = textBoxUserEmail.Text;          // if it is save these details to the computer for next launch
                Properties.Settings.Default.Password = textBox_Password.Text;
                Properties.Settings.Default.serverIP = textBoxServerIP.Text;
                Properties.Settings.Default.serverPort = textBoxServerPort.Text;
                Properties.Settings.Default.Save();                                 
                checkBoxRememberMe.Checked = true;
            }
            Properties.Settings.Default.serverIP = textBoxServerIP.Text;            // get server ip from textbox to pass to clientRun()
            Properties.Settings.Default.serverPort = textBoxServerPort.Text;        // same but for port

            checkIsEmail();                                                         // check if email counts as a valid email address

            if (emailIsEmail)                                                       // if it is create/update service.ini and run the game
            {                                                                       // if its not it wont do anything else
                serviceIni();
                clientRun();
            }
        }

        private void clientRun()
        {
            /*******************************************************************
             * Setup a command prompt and pass the commands to it
             *******************************************************************/
            Process p = new Process();                                                  // Setup Process variable
            ProcessStartInfo info = new ProcessStartInfo();                             // setup start info for the process
            info.FileName = "cmd.exe";                                                  // file to run - in this case "cmd.exe"
            info.RedirectStandardInput = true;                                          // so the "cmd.exe" can accept the StreamWriter from below
            info.UseShellExecute = false;                                               // dont use the operating system shell to start the process.
            info.CreateNoWindow = true;                                                 // hides the command prompt window
            
            p.StartInfo = info;
            p.Start();                                                                  // start cmd.exe

            using (StreamWriter sw = p.StandardInput)                                   // pass on these commands
            {
                if (sw.BaseStream.CanWrite)                                             // checks if able to write to cmd.exe before trying
                {                                                                       // if returns true pass on commands
                    sw.WriteLine("cd bin64");                                           // everything inside the "" is printed to the cmd.exe - one for each line
                    sw.WriteLine("start BlackDesert64.exe" + " " + textBoxUserEmail.Text + "," + textBox_Password.Text);
                }
            }
        }

        private void serviceIni() // builds the servce.ini file
        {

    var MyIni = new IniFile("service.ini");

            /*******************************************************************
             *  if TYPE is not set under the section [SERVICE]
             *  create it and set it to NA like this
             *  
             *  [SERVICE]
             *  TYPE=NA
             *******************************************************************/

            if (!MyIni.KeyExists("TYPE", "SERVICE"))            // check if key exists
            {
                MyIni.Write("TYPE", "NA", "SERVICE");           // if it doesnt add it and set it
            }
            if (!MyIni.KeyExists("RES", "SERVICE"))             // repeat
            {
                MyIni.Write("RES", "_EN_", "SERVICE");
            }
            if (!MyIni.KeyExists("nationType", "SERVICE"))
            {
                MyIni.Write("nationType", "1", "SERVICE");
            }
            if (!MyIni.KeyExists("damageMeter", "SERVICE"))
            {
                MyIni.Write("damageMeter", "1", "SERVICE");
            }
            if (!MyIni.KeyExists("AUTHENTIC_DOMAIN", "NA"))
            {
                MyIni.Write("AUTHENTIC_DOMAIN", "127.0.0.1", "NA"); 
            }
            if (!MyIni.KeyExists("AUTHENTIC_PORT", "NA"))
            {
                MyIni.Write("AUTHENTIC_PORT", "8888", "NA");
            }
            if (!MyIni.KeyExists("PATCH_URL", "NA"))
            {
                MyIni.Write("PATCH_URL", "http://downtest1.black.game.daumserver.com/patch/", "NA");
            }
            
            MyIni.Write("AUTHENTIC_DOMAIN", textBoxServerIP.Text, "NA"); // If server ip/port keys do exist in the file override them with the ip/port from the textboxes
            MyIni.Write("AUTHENTIC_PORT", textBoxServerPort.Text, "NA"); // same as above
        }

        private void checkIsEmail()
        {
            /*******************************************************************
             * Simple Regex verification that check's if the text entered in 
             * the variable "textBoxUserEmail.Text" has atleast a "@" and a "."
             *******************************************************************/
                                                            // setup Regex dump to check against
            Regex emailRegex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");

            if (emailRegex.IsMatch(textBoxUserEmail.Text))  // checks if input is infact an email address
            {                
                emailIsEmail = true;                        // it matches so set variable to true
            }
            else                                            // does not match so show error instead
            {
                DialogResult result = MessageBox.Show(" Username needs to be a valid email address.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

}