using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Collections.Generic;

namespace FearLand_NoCrosshair_Patcher
{
    public partial class MainWnd : Form
    {
        private const string GAME_FFL_ORIGINAL_MD5 = "03-20-D6-8A-CF-B7-EE-7B-47-84-ED-43-B1-13-F0-A0";
        private const string GAME_FFL_FULL_MD5 = "D3-63-9B-F0-4A-BA-BC-72-46-DF-5A-0A-08-92-40-8B";
        private const string GAME_FFL_ALLRH_MD5 = "83-72-32-3F-FC-EE-BF-D0-64-70-70-64-AC-00-C0-43";

        private const string GAME_HM2_1_01_JPN_v1_MD5 = "ED-E2-A1-0F-E3-72-21-D3-99-4F-31-55-3B-3A-4E-F5";
        private const string GAME_HM2_1_01_JPN_v2_MD5 = "FB-49-3E-DA-4C-BC-8A-08-66-FA-73-3F-B7-84-F0-E5";
        private const string GAME_HM2_JCONFIG_1_00_BGR_MD5 = "9B-55-67-BD-A6-99-41-92-3F-EB-3F-2E-05-61-9C-68";
        private const string GAME_HM2_JCONFIG_1_01_JPN_v1_MD5 = "26-4D-67-1B-83-28-2A-09-70-1B-27-F2-24-9C-5D-0D";
        private const string GAME_HM2_JCONFIG_1_01_JPN_v2_MD5 = "0E-8F-49-EB-44-8F-F7-C9-BD-44-89-40-A5-3A-C1-6A";

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

                        else if (_sMD5.Equals(GAME_HM2_1_01_JPN_v1_MD5))
                            WriteLog("\"Haunted Museum 2 v1.01 JPN first release\" TTX game.exe detected !\nDemulShooter command: demulshooter.exe -target=ttx -rom=hmuseum2");

                        else if (_sMD5.Equals(GAME_HM2_1_01_JPN_v2_MD5))
                            WriteLog("\"Haunted Museum 2 v1.01 JPN first release\" TTX game.exe detected !\nDemulShooter command: demulshooter.exe -target=ttx -rom=hmuseum2");

                        else if (_sMD5.Equals(GAME_HM2_JCONFIG_1_00_BGR_MD5))
                            WriteLog("\"Haunted Museum 2 v1.00 BGR for Jconfig\" TTX game.exe detected !\nDemulShooter command: demulshooter.exe -target=ttx -rom=hmuseum2");

                        else if (_sMD5.Equals(GAME_HM2_JCONFIG_1_01_JPN_v1_MD5))
                            WriteLog("\"Haunted Museum 2 v1.01 JPN for Jconfig\" TTX game.exe detected !\nDemulShooter command: demulshooter.exe -target=ttx -rom=hmuseum2");

                        else if (_sMD5.Equals(GAME_HM2_JCONFIG_1_01_JPN_v2_MD5))
                            WriteLog("\"Haunted Museum 2 v1.01 JPN_v2 for Jconfig\" TTX game.exe detected !\nDemulShooter command: demulshooter.exe -target=ttx -rom=hmuseum2");

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

                if (_sMD5 == GAME_HM2_1_01_JPN_v1_MD5)
                {
                    //Remove crosshairs textures during gameplay only
                    RemoveCrosshair(0x0007197D, 0x00071549);
                    //Change Ammo font size
                    ChangeammoFontSize(0x0029CE6C, 0x0029C0E0);
                    //Force game to read our axis values for Ammo display
                    PatchAmmoDisplayPosition(0x00071651, 0x000716EF, 0x00071785, 0x007265E0);
                    //Codecave : File offset 0x264EE0
                    CreateCodecave(0x00071584, 0x00264EE0, 0x008A9350, 0x007265E0);   
                }
                else if (_sMD5 == GAME_HM2_1_01_JPN_v2_MD5)
                {
                    if (MessageBox.Show("Impossible to crosshair-patch this version of the game. \nPlease try with another release.", "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.OK)
                        return;

                    //Remove crosshairs textures during gameplay only
                    RemoveCrosshair(0x000712AD, 0x00070E79);
                    //Change Ammo font size
                    ChangeammoFontSize(0x00299F9C, 0x00299220);
                    //Force game to read our axis values for Ammo display
                    PatchAmmoDisplayPosition(0x00070F81, 0x0007101F, 0x000710B5, 0x007232E0);
                    //Codecave : File offset  0x26219E
                    //CreateCodecave(0x00070EB4, 0x0026219E, 0x008A6040, 0x007232E0); NOT ENOUGH PLACE !!!  
                }
                else if(_sMD5 == GAME_HM2_JCONFIG_1_00_BGR_MD5)
                {
                    //Remove crosshairs textures during gameplay only
                    RemoveCrosshair(0x0007197D, 0x00071549);
                    //Change Ammo font size
                    ChangeammoFontSize(0x0029CE6C, 0x0029C0E0);
                    //Force game to read our axis values for Ammo display
                    PatchAmmoDisplayPosition(0x00071651, 0x000716EF, 0x00071785, 0x007265E0);
                    //Codecave : File offset  0x264F1F
                    CreateCodecave(0x00071584, 0x00264F1F, 0x008A9350, 0x007265E0);   
                }
                else if (_sMD5 == GAME_HM2_JCONFIG_1_01_JPN_v1_MD5)
                {
                    //Remove crosshairs textures during gameplay only
                    RemoveCrosshair(0x0071B9D, 0x0071769);
                    //Change Ammo font size
                    ChangeammoFontSize(0x0031417C, 0x003133F0);
                    //Force game to read our axis values for Ammo display
                    PatchAmmoDisplayPosition(0x00071871, 0x0007190F, 0x000719A5, 0x007AF6E0);
                    //Codecave : File offset  0x2D4ABF
                    CreateCodecave(0x000717A4, 0x002D4ABF, 0x00A0EFB0, 0x007AF6E0);   
                }
                else if (_sMD5 == GAME_HM2_JCONFIG_1_01_JPN_v2_MD5)
                {
                    if (MessageBox.Show("Impossible to crosshair-patch this version of the game. \nPlease try with another release.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.OK)
                        return;

                    //Remove crosshairs textures during gameplay only
                    RemoveCrosshair(0x00299F9C, 0x00299220);
                    //Force game to read our axis values for Ammo display
                    PatchAmmoDisplayPosition(0x00070F81, 0x0007101F, 0x000710B5, 0x007232E0);
                    //Codecave : File offset  0x2621CF
                    //CreateCodecave(0x00070EB4, 0x002621CF, 0x008A6040, 0x007232E0);   NOT ENOUGH PLACE !!!
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

        //Removing the crosshair design at runtime by replacing the instruction :
        // - push    offset aSight   ; "sight"
        //by :
        // - push 00
        // - nop
        // - nop
        // - nop
        private void RemoveCrosshair(int Sight00_TextureFileOffset, int Sight_TextureFileOffset)
        {
            ModifyBytes(Sight00_TextureFileOffset, new byte[] { 0x6A, 0x00, 0x90, 0x90, 0x90 });
            ModifyBytes(Sight_TextureFileOffset, new byte[] { 0x6A, 0x00, 0x90, 0x90, 0x90 });
        }

        //Changing the Ammo display font by modifying float value :
        // - float1 : 0,33000001 -> 0,8
        // - float2 : 0,23 -> 0,5
        private void ChangeammoFontSize(int Float1_FileOffset, int Float2_fileOffset)
        {
            ModifyBytes(Float1_FileOffset, new byte[] { 0xCD, 0xCC, 0x4C, 0x3F });
            ModifyBytes(Float2_fileOffset, new byte[] { 0x00, 0x00, 0x00, 0x3F });
        }

        //Force the game to read Ammo position calculated in the custom codecave
        private void PatchAmmoDisplayPosition(int PatchOffset1, int PatchOffset2, int PatchOffset3, int Data_FileOffset)
        {
            List<Byte> bPatch = new List<Byte>();
            bPatch.AddRange(new byte[] { 0x83, 0xC4, 0x08, 0xFF, 0x35 });
            bPatch.AddRange(BitConverter.GetBytes(Data_FileOffset + 0x08));
            bPatch.AddRange(new byte[] { 0xFF, 0x35 });
            bPatch.AddRange(BitConverter.GetBytes(Data_FileOffset));

            ModifyBytes(PatchOffset1, bPatch.ToArray());
            ModifyBytes(PatchOffset2, bPatch.ToArray());
            ModifyBytes(PatchOffset3, bPatch.ToArray());
        }

        //Creating the CodeCave and generating JmpTo and JmpBack instructions
        //This codecave will read Width/Height of the Viewport and set the position of ammo for P1 / P2 in the memory to be read later
        private void CreateCodecave(int Injection_FileOffset, int CodeCave_FileOffset, int CaveReadVariable_Offset, int CaveWriteVariable_Offset)
        {
            int JmpValue = CodeCave_FileOffset - Injection_FileOffset - 5;

            List<Byte> bCodeCave = new List<Byte>();
            bCodeCave.Add(0xE9);
            bCodeCave.AddRange(BitConverter.GetBytes(JmpValue));

            //JMP to codecave
            ModifyBytes(Injection_FileOffset, bCodeCave.ToArray());

            bCodeCave.Clear();
            bCodeCave.AddRange(new Byte[] { 0x83, 0xEC, 0x10, 0xF3, 0x0F, 0x7F, 0x04, 0x24, 0x83, 0xEC, 0x10, 0xF3, 0x0F, 0x7F, 0x0C, 0x24, 0x83, 0xF8, 0x00, 0x0F, 0x86, 0x17, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0x05 });
            bCodeCave.AddRange(BitConverter.GetBytes(CaveReadVariable_Offset + 0xC));
            bCodeCave.AddRange(new Byte[] { 0xB8, 0x28, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0xC8, 0xF3, 0x0F, 0x5C, 0xC1, 0xEB, 0x09, 0xB8, 0x28, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0xC0, 0xF3, 0x0F, 0x11, 0x05 });
            bCodeCave.AddRange(BitConverter.GetBytes(CaveWriteVariable_Offset));
            bCodeCave.AddRange(new Byte[] { 0xF3, 0x0F, 0x2A, 0x05 });
            bCodeCave.AddRange(BitConverter.GetBytes(CaveReadVariable_Offset));
            bCodeCave.AddRange(new Byte[] { 0xB8, 0x78, 0x00, 0x00, 0x00, 0xF3, 0x0F, 0x2A, 0xC8, 0xF3, 0x0F, 0x5C, 0xC1, 0xF3, 0x0F, 0x11, 0x05 });
            bCodeCave.AddRange(BitConverter.GetBytes(CaveWriteVariable_Offset + 0x08));
            bCodeCave.AddRange(new Byte[] { 0xB8, 0x93, 0x24, 0x49, 0x92, 0xF3, 0x0F, 0x6F, 0x0C, 0x24, 0x83, 0xC4, 0x10, 0xF3, 0x0F, 0x6F, 0x04, 0x24, 0x83, 0xC4, 0x10, 0xE9 });

            int ReturnJmpValue = (Injection_FileOffset + 5) - (CodeCave_FileOffset + bCodeCave.Count + 4);
            bCodeCave.AddRange(BitConverter.GetBytes(ReturnJmpValue));

            //Codecave :
            ModifyBytes(CodeCave_FileOffset, bCodeCave.ToArray());            
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
