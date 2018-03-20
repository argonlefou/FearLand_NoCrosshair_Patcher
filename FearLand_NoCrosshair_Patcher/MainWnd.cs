using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace FearLand_NoCrosshair_Patcher
{
    public partial class MainWnd : Form
    {
        private const string GAME_FFL_ORIGINAL_MD5 = "03-20-D6-8A-CF-B7-EE-7B-47-84-ED-43-B1-13-F0-A0";
        private const string GAME_FFL_FULL_MD5 = "D3-63-9B-F0-4A-BA-BC-72-46-DF-5A-0A-08-92-40-8B";
        private const string GAME_FFL_ALLRH_MD5 = "83-72-32-3F-FC-EE-BF-D0-64-70-70-64-AC-00-C0-43";
        private const string GAME_HM2_MD5 = "ED-E2-A1-0F-E3-72-21-D3-99-4F-31-55-3B-3A-4E-F5";

        private string _sMD5 = string.Empty;
        private string _TargetFile = string.Empty;

        private uint imageBase;
        private uint virtualAddress;
        private uint virtualSize;
        private uint _RawAddress;
        private uint _RawSize;

        private long addressVirtualSize;


        public MainWnd()
        {
            InitializeComponent();
        }

        private void Btn_Open_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select your 'Fright Fear Land' game.exe file";
            openFileDialog1.Filter = "exe files (*exe) | *.exe";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Btn_Patch.Enabled = true;
                Btn_PatchCrosshair.Enabled = true;
                Txt_Log.Clear();
                Txt_File.Text = openFileDialog1.FileName;
                
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(openFileDialog1.FileName))
                    {  
                        //Getting md5 calculation of destination file
                        _sMD5 = BitConverter.ToString(md5.ComputeHash(stream));

                        //Select according source file to apply patch
                        if (_sMD5.Equals(GAME_FFL_ORIGINAL_MD5))
                            WriteLog("Original Fright Fear Land game.exe detected !\nDemulShooter command: demulshooter.exe -target=globalvr -rom=fearland");

                        else if (_sMD5.Equals(GAME_FFL_FULL_MD5))
                            WriteLog("\"Full\" Fright Fear Land game.exe detected !\nDemulShooter command: demulshooter.exe -target=globalvr -rom=fearland");

                        else if (_sMD5.Equals(GAME_FFL_ALLRH_MD5))
                            WriteLog("\"Game Loader All RH\" Fright Fear Land game.exe detected !\nDemulShooter command: demulshooter.exe -target=globalvr -rom=fearland");

                        else if (_sMD5.Equals(GAME_HM2_MD5))
                            WriteLog("\"Haunted Museum 2\" TTX game.exe detected !\nDemulShooter command: demulshooter.exe -target=ttx -rom=hmuseum2");


                        else
                            WriteLog("Unknown game.exe version, proceed with caution !");                        
                    }
                }
            }
            else
            {
                Btn_Patch.Enabled = false;
                Btn_PatchCrosshair.Enabled = false;
            }
        }

        private void Btn_Patch_Click(object sender, EventArgs e)
        {
            WriteLog("");
            PatchHeader();
        }

        private void Btn_PatchCrosshair_Click(object sender, EventArgs e)
        {
            WriteLog("");
            if(PatchHeader())
                PatchCrosshair();
        }

        private bool PatchHeader()
        {         
            GetRelocTableAddress();
            if (_RawAddress == 0x00 || _RawSize == 0x00)
            {
                WriteLog("Can't get the relocation table from the exe.");
                WriteLog("Patch canceled !");
                return false;
            }

            WriteLog("Relocation table found at 0x" + _RawAddress.ToString("X8"));           
                
            try
            {
                WriteLog("Backuping original file to \"" + Txt_File.Text +  ".bak\"  ...");    
                File.Copy(Txt_File.Text, Txt_File.Text+ ".bak");
                WriteLog("Patching header...");
                using (BinaryWriter fileWriter = new BinaryWriter(File.Open(Txt_File.Text, FileMode.Open)))
                {
                    fileWriter.BaseStream.Position = _RawAddress;
                    byte[] Buffer = new byte[_RawSize];
                    fileWriter.Write(Buffer);
                }
                
                WriteLog("\n---- HEADER PATCH SUCCESS ! ----\n\n");
                Btn_Patch.Enabled = false;
                Btn_PatchCrosshair.Enabled = false;
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("Header patching error :");
                WriteLog(ex.Message.ToString());
                return false;
            }
        }

        private void PatchCrosshair()
        {
            try
            {
                WriteLog("Applying \"No-Crosshair Patch\" ...");

                if (_sMD5 == GAME_HM2_MD5)
                {
                    //Remove crosshairs textures during gameplay only
                    ModifyBytes(0x0007197D, new byte[] { 0x6A, 0x00, 0x90, 0x90, 0x90 });
                    ModifyBytes(0x00071549, new byte[] { 0x6A, 0x00, 0x90, 0x90, 0x90 });
                    //Change Ammo font size
                    ModifyBytes(0x0029CE6C, new byte[] { 0xCD, 0xCC, 0x4C, 0x3F });
                    ModifyBytes(0x0029C0E0, new byte[] { 0x00, 0x00, 0x00, 0x3F });
                    //Force game to read our axis values for Ammo display
                    ModifyBytes(0x00071651, new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35, 0xE8, 0x65, 0x72, 0x00, 0xFF, 0x35, 0xE0, 0x65, 0x72, 0x00 });
                    ModifyBytes(0x000716EF, new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35, 0xE8, 0x65, 0x72, 0x00, 0xFF, 0x35, 0xE0, 0x65, 0x72, 0x00 });
                    ModifyBytes(0x00071785, new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35, 0xE8, 0x65, 0x72, 0x00, 0xFF, 0x35, 0xE0, 0x65, 0x72, 0x00 });
                    //JMP to codecave
                    ModifyBytes(0x00071584, new byte[] { 0xE9, 0x57, 0x39, 0x1F, 0x00 });
                    //Codecave :
                    ModifyBytes(0x00264EE0, new byte[] { 0x83, 0xEC, 0x10, 0xF3, 0x0F, 0x7F, 0x04, 0x24, 0x83, 0xEC, 0x10, 0xF3, 0x0F, 0x7F, 0x0C, 0x24, 0x83, 0xF8, 0x00, 0x0F, 0x86, 0x17, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0x05, 0x5C, 0x93, 0x8A, 0x00, 0xB8, 0x28, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0xC8, 0xF3, 0x0F, 0x5C, 0xC1, 0xEB, 0x09, 0xB8, 0x28, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0xC0, 0xF3, 0x0F, 0x11, 0x05, 0xE0, 0x65, 0x72, 0x00, 0xF3, 0x0F, 0x2A, 0x05, 0x50, 0x93, 0x8A, 0x00, 0xB8, 0x78, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0xC8, 0xF3, 0x0F, 0x5C, 0xC1, 0xF3, 0x0F, 0x11, 0x05, 0xE8, 0x65, 0x72, 0x00, 0xB8, 0x93, 0x24, 0x49, 0x92, 0xF3, 0x0F, 0x6F, 0x0C, 0x24, 0x83, 0xC4, 0x10, 0xF3, 0x0F, 0x6F, 0x04, 0x24, 0x83, 0xC4, 0x10, 0xE9, 0x31, 0xC6, 0xE0, 0xFF });
                }
                else
                {
                    //Remove crosshairs textures during gameplay only
                    ModifyBytes(0x00071B9D, new byte[] { 0x6A, 0x00, 0x90, 0x90, 0x90 });
                    ModifyBytes(0x00071769, new byte[] { 0x6A, 0x00, 0x90, 0x90, 0x90 });
                    //Change Ammo font size
                    ModifyBytes(0x0031417C, new byte[] { 0xCD, 0xCC, 0x4C, 0x3F });
                    ModifyBytes(0x003133F0, new byte[] { 0x00, 0x00, 0x00, 0x3F });
                    //Force game to read our axis values for Ammo display
                    ModifyBytes(0x00071871, new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35, 0x28, 0xF7, 0x7A, 0x00, 0xFF, 0x35, 0x20, 0xF7, 0x7A, 0x00 });
                    ModifyBytes(0x0007190F, new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35, 0x28, 0xF7, 0x7A, 0x00, 0xFF, 0x35, 0x20, 0xF7, 0x7A, 0x00 });
                    ModifyBytes(0x000719A5, new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35, 0x28, 0xF7, 0x7A, 0x00, 0xFF, 0x35, 0x20, 0xF7, 0x7A, 0x00 });
                    //JMP to codecave
                    ModifyBytes(0x000717A4, new byte[] { 0xE9, 0xE7, 0x32, 0x26, 0x00 });
                    //Data segment : our values
                    ModifyBytes(0x003AD700, new byte[] { 0x00, 0x00, 0x20, 0x42 }); //40.0
                    ModifyBytes(0x003AD708, new byte[] { 0x00, 0x00, 0x20, 0x42 }); //40.0
                    ModifyBytes(0x003AD710, new byte[] { 0x00, 0x00, 0xF0, 0x42 }); //120.0
                    //Codecave :
                    ModifyBytes(0x002D4A90, new byte[] { 0x83, 0xEC, 0x10, 0xF3, 0x0F, 0x7F, 0x04, 0x24, 0x83, 0xEC, 0x10, 0xF3, 0x0F, 0x7F, 0x0C, 0x24, 0x83, 0xF8, 0x00, 0x0F, 0x86, 0x16, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0x05, 0xBC, 0xEF, 0xA0, 0x00, 0xF3, 0x0F, 0x10, 0x0D, 0x08, 0xF7, 0x7A, 0x00, 0xF3, 0x0F, 0x5C, 0xC1, 0xEB, 0x08, 0xF3, 0x0F, 0x10, 0x05, 0x00, 0xF7, 0x7A, 0x00, 0xF3, 0x0F, 0x11, 0x05, 0x20, 0xF7, 0x7A, 0x00, 0xF3, 0x0F, 0x2A, 0x05, 0xB0, 0xEF, 0xA0, 0x00, 0xF3, 0x0F, 0x10, 0x0D, 0x10, 0xF7, 0x7A, 0x00, 0xF3, 0x0F, 0x5C, 0xC1, 0xF3, 0x0F, 0x11, 0x05, 0x28, 0xF7, 0x7A, 0x00, 0xB8, 0x93, 0x24, 0x49, 0x92, 0xF3, 0x0F, 0x6F, 0x0C, 0x24, 0x83, 0xC4, 0x10, 0xF3, 0x0F, 0x6F, 0x04, 0x24, 0x83, 0xC4, 0x10, 0xE9, 0xA4, 0xCC, 0xD9, 0xFF });

                }
                WriteLog("\n---- NO-CROSSHAIR PATCH SUCCESS ! ----\n\n");
                Btn_Patch.Enabled = false;
                Btn_PatchCrosshair.Enabled = false;
            }
            catch (Exception ex)
            {
                WriteLog("Header patching error :");
                WriteLog(ex.Message.ToString());
            }
            
        }

        private void GetRelocTableAddress()
        {
            BinaryReader br = new BinaryReader(new FileStream(Txt_File.Text, FileMode.Open, FileAccess.Read, FileShare.Read));

            if (br.ReadInt16() != 0x5A4D) // MZ signature
            {
                br.Close();
                return;
            }

            br.BaseStream.Seek(0x3C, SeekOrigin.Begin); // go to ptr to COFF File Header
            br.BaseStream.Seek(br.ReadInt32(), SeekOrigin.Begin); // go to COFF File Header

            if (br.ReadInt32() != 0x00004550) // PE\0\0
            {
                br.Close();
                return;
            }

            br.ReadUInt16(); // machine ID. Ignored
            uint numbersOfSections = br.ReadUInt16();

            br.BaseStream.Seek(20 - 4, SeekOrigin.Current); // go to magic number (PE or PE+ = x86 or x64)

            if (br.ReadInt16() != 0x010B)
            {
                br.Close();
                return;
            }

            br.BaseStream.Seek(26, SeekOrigin.Current); // go to ImageBase
            imageBase = br.ReadUInt32();
            br.BaseStream.Seek(64 + 40, SeekOrigin.Current); // go to Data Directories -> Base Relocation Table

            virtualAddress = br.ReadUInt32();
            addressVirtualSize = br.BaseStream.Position;
            virtualSize = br.ReadUInt32();

            br.BaseStream.Seek(80, SeekOrigin.Current); // jump to Section Table

            // find the RAW address/size
            _RawAddress = 0;
            _RawSize = 0;

            for (int i = 0; i < numbersOfSections; i++)
            {
                br.BaseStream.Seek(12, SeekOrigin.Current);

                if (br.ReadUInt32() == virtualAddress)
                {
                    _RawSize = br.ReadUInt32();
                    _RawAddress = br.ReadUInt32();
                    break;
                }

                br.BaseStream.Seek(24, SeekOrigin.Current); // place the pointer to the next section
            }

            br.Close();
        }

        private void ModifyBytes(int Offset, byte[] Buffer)
        {
            using (BinaryWriter fileWriter = new BinaryWriter(File.Open(Txt_File.Text, FileMode.Open)))
            {
                fileWriter.BaseStream.Position = Offset;
                fileWriter.Write(Buffer);
            }
        }

        private void WriteLog(string Text)
        {
            Txt_Log.AppendText(Text + "\n");
        }
    }
}
