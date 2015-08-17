using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace KMPExpander
{
    
    public partial class KMPEd : Form
    {
        const string Ver = "v1.6";
        KMP keiempi;
        KMP bin;
        int SizeOfKMP;
        byte[] PointSizes = new byte[18] { 255, 24, 72, 20, 28, 24, 16, 64, 255, 255, 255, 28, 255, 255, 255, 255, 24, 22 }; //Size of points inside the sections. If set to 255, the program will ignore anything related to it, including the CSV operations.
        byte[] NumData = new byte[18] { 255, 6, 35, 5, 14, 12, 15, 23, 4, 255, 255, 8, 255, 255, 255, 255, 6, 16 }; //Number of fields on each section
        string[,] SecMagic = new string[2, 18] {
                                                {"TPTK","TPNE","HPNE","TPTI","HPTI","TPKC","HPKC","JBOG","ITOP","AERA","EMAC","TPGJ","TPNC","TPSM","IGTS","SROC","TPLG","HPLG"},
                                                {"KTPT (Kart Point)","ENPT (Enemy Routes)","ENPH (Enemy Routes' Sections)","ITPT (Item Routes)","ITPH (Item Routes' Sections)","CKPT (Checkpoints)","CKPH (Checkpoints' Sections)","GOBJ (Global Objects)","POTI (Routes)","AREA","CAME (Camera)","JGPT (Respawn Points)","CNPT (Cannon Points)","MSPT (Mission Points)","STGI","CORS","GLPT (Glider Points)","GLPH (Glider Points' Sections)"}
                                              };
        int[] Offsets= new int[18];
        string csv = ""; // Comma Separated Values
        string[] csv_parse;
        const string csv_intro = "#Exported from KMP Expander (" + Ver + ") - made by Ermelber\n";


        string GroupBy4(int pos)
        {
            char[] TempStr = new char[4];
            for (int ii = 0; ii < 4; ii++)
            {
                TempStr[ii] = (char)keiempi[pos + ii];
            }
                
            string s = new string(TempStr);
            return s;
        }

        string GroupBy4Bin(int pos)
        {
            char[] TempStr = new char[4];
            for (int ii = 0; ii < 4; ii++) {
                TempStr[ii] = (char)bin[pos + ii];
            }
            
            string s = new string(TempStr);
            return s;
        }

        void getSections()
        {
            int ofs = 0;
            for (int pos = 0; (pos < SizeOfKMP) && (ofs < 18); pos++)
                if (SecMagic[0, ofs] == GroupBy4(pos))
                {
                    Offsets[ofs] = pos - 0x58; //Subtract the header size to the section offset
                    ofs++;
                }
        }

        

        //FILE CHECKS
        void EnableEditing()
        {
            comboBox1.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            label2.Visible = true;
            filesize_box.Text = SizeOfKMP.ToString() + " bytes";
        }

        bool checkSignature()
        {
            bool Check;
            Check = (GroupBy4(0) == "DMDC") && (GroupBy4(88) == SecMagic[0,0]);
            return Check;
        }

        bool checkNumSections(ref int num)
        {
            bool Check;
            for (int pos = 0; (pos < SizeOfKMP - 4) && (num<18); pos++)
                if (SecMagic[0, num] == GroupBy4(pos)) num++;
            Check = (num == 18);
            return Check;
        }

        bool checkFileSize()
        {
            bool Check;
            int filesize;
            filesize = (int)(keiempi[4] | keiempi[4 + 1] << 8);
            Check = (filesize != SizeOfKMP);
            return Check;
        }

        void UpdateFileSize()
        {
            keiempi[4] = (byte)((int)SizeOfKMP & 0xFF);
            keiempi[5] = (byte)(((int)SizeOfKMP >> 8) & 0xFF);
        }

        bool checkSections()
        {
            bool Check = false;
            byte pos = 0x10;
            getSections();
            for (int i = 0; (i < 18) && (Check==false); i++, pos += 4)
                Check = ((keiempi[pos] | keiempi [pos+1] << 8) != Offsets[i]);
            return Check;
        }

        void UpdateSections()
        {
            getSections();
            byte pos = 0x10;
            for (int i = pos; i < 0x58; i++)
                keiempi[i] = 0;
            for (int i = 0; i < 18 ; i++, pos += 4)
            {
                keiempi[pos] = (byte)((int)Offsets[i] & 0xFF);
                keiempi[pos+1] = (byte)(((int)Offsets[i] >> 8) & 0xFF);
            }
        }

        void CheckHeader()
        {
            int num = 0;
            if (checkSignature())
            {
                if (checkNumSections(ref num))
                {
                    if (checkFileSize())
                        MessageBox.Show("Internal Filesize isn't correct. It will be adjusted now.");
                    if (checkSections())
                        MessageBox.Show("There seems to be some inconstistencies between the header and sections. It will be corrected now.");
                    UpdateFileSize();
                    UpdateSections();
                }
                else MessageBox.Show(SecMagic[1, num]+" Section seems to be missing!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("This KMP's Header doesn't look like a MK7 one!\nCheck if the Signature is correct (It should be DMDC) or if the Header's size is 0x58", "Warning",MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        bool CheckBinaryMagic(byte index)
        {
            return (GroupBy4Bin(0) == SecMagic[0,index]);
        }

        bool checkNumPoints(byte index,ref short numpt)
        {
            int size = bin.Data.Length;
            numpt = (short)((size - 8) / PointSizes[index]);
            return ((size - 8) % PointSizes[index] == 0);
        }

        void checkSectPoint()
        {
            bool tf = true;
            int size = 0;
            short numpt = 0;
            for (int i=0;(i<18)&&(tf);i++)
            {
                if (PointSizes[i] != 255)
                {
                    if (i == 17)
                        size = SizeOfKMP - (Offsets[i] + 0x58);
                    else size = (Offsets[i + 1] - Offsets[i]);
                    numpt = (short)((size - 8) / PointSizes[i]);
                    tf = (((size - 8) % PointSizes[i]) == 0);
                    if (tf)
                    {
                        if ((keiempi[Offsets[i] + 0x58 + 4] | keiempi[Offsets[i] + 0x58 + 5] << 8) != numpt)
                        {
                            MessageBox.Show(SecMagic[1, i] + "'s amount of points is incorrect. It'll be corrected automatically");
                            keiempi[Offsets[i] + 0x58 + 4] = (byte)((int)numpt & 0xFF);
                            keiempi[Offsets[i] + 0x58 + 5] = (byte)(((int)numpt >> 8) & 0xFF);
                        }
                    }
                    else MessageBox.Show(SecMagic[1, i] + " seems to be damaged! KMP won't be loaded.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (tf) EnableEditing();
        }

        //File Operations
        /// <summary>
        /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        private static bool IsHexDigit(Char c)
        {
            int numChar;
            int numA = Convert.ToInt32('A');
            int num1 = Convert.ToInt32('0');
            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);
            if (numChar >= numA && numChar < (numA + 6))
                return true;
            if (numChar >= num1 && numChar < (num1 + 10))
                return true;
            return false;
        }

        /// <summary>
        /// Converts 1 or 2 character string into equivalant byte value
        /// </summary>
        /// <param name="hex">1 or 2 character string</param>
        /// <returns>byte</returns>

        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

        /// <summary>
        /// Creates a byte array from the hexadecimal string. Each two characters are combined
        /// to create one byte. First two hexadecimal characters become first byte in returned array.
        /// Non-hexadecimal characters are ignored. 
        /// </summary>
        /// <param name="hexString">string to convert to byte array</param>
        /// <param name="discarded">number of characters in string ignored</param>
        /// <returns>byte array, in the same left-to-right order as the hexString</returns>
        private static byte[] GetBytes(string hexString, out int discarded)
        {
            discarded = 0;
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
                else
                    discarded++;
            }
            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                discarded++;
                newString = newString.Substring(0, newString.Length - 1);
            }
            int byteLength = newString.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }

        public static byte[] StringToByte(string a)
        {
            byte[] buffer;
            int discarded;
            buffer = GetBytes(a, out discarded);
            return buffer;
        }

        float Rad2Deg(float angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        float Deg2Rad(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        
        //Gets which Object/CAME etc uses a route.
        void UsedBy(ref short[,] mat)
        {
            //Get Object's routes
            short temp_route;
            short numpt_gobj = (short)(keiempi[Offsets[7] + 0x58 + 4] | keiempi[Offsets[7] + 0x58 + 5] << 8);
            for (int i = 0; i < numpt_gobj; i++)
            {
                temp_route = (short)(keiempi[Offsets[7] + 0x58 + 48 + i * PointSizes[7]] | keiempi[Offsets[7] + 0x58 + 49 + i * PointSizes[7]] << 8);
                if (temp_route > -1)
                {
                    mat[temp_route,0]=(short)i;
                    mat[temp_route,1]=0;  //Used by a GOBJ
                }
            }
        }

        void Inject(byte[] data)
        {
            var kmp_lst = new List<byte>();
            kmp_lst.AddRange(keiempi.Data);
            int size;
            if (comboBox1.SelectedIndex == 17)
                size = SizeOfKMP - (Offsets[comboBox1.SelectedIndex] + 0x58);
            else size = (Offsets[comboBox1.SelectedIndex + 1] - Offsets[comboBox1.SelectedIndex]);
            kmp_lst.RemoveRange((Offsets[comboBox1.SelectedIndex] + 0x58), size);
            kmp_lst.InsertRange((Offsets[comboBox1.SelectedIndex] + 0x58), data);
            keiempi.Data = kmp_lst.ToArray();
            getSections();
            SizeOfKMP = keiempi.Data.Length;
            UpdateFileSize();
            UpdateSections();
            filesize_box.Text = SizeOfKMP.ToString() + " bytes";
        }

        void ImportBin()
        {
            short numpt = 0;
            if (CheckBinaryMagic((byte)comboBox1.SelectedIndex))
            {
                if (PointSizes[comboBox1.SelectedIndex] != 255)
                {
                    if (checkNumPoints((byte)comboBox1.SelectedIndex, ref numpt))
                    {
                        if ((bin[4] | bin[5] << 8) != numpt)
                        {
                            MessageBox.Show(SecMagic[1, comboBox1.SelectedIndex] + "'s amount of points is incorrect. It'll be corrected automatically");
                            bin[4] = (byte)((int)numpt & 0xFF);
                            bin[5] = (byte)(((int)numpt >> 8) & 0xFF);
                        }
                        Inject(bin.Data);
                        MessageBox.Show(SecMagic[1, comboBox1.SelectedIndex] + " injected successfully!");
                    }
                    else MessageBox.Show("This binary file seems damaged.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    //This is for unhandled sections I have still to research.
                    Inject(bin.Data);
                    MessageBox.Show(SecMagic[1, comboBox1.SelectedIndex] + " injected successfully!");
                }
            }
            else MessageBox.Show("This file's Magic doesn't match " + SecMagic[1, comboBox1.SelectedIndex] + "'s one!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void ExportCSV()
        {
            short numpt = 0;
            short numpoti = 0;  //Used only for POTI!
            int Section = comboBox1.SelectedIndex;
            numpt = (short)(keiempi[Offsets[Section] + 0x58 + 4] | keiempi[Offsets[Section] + 0x58 + 5] << 8);
            numpoti = (short)(keiempi[Offsets[Section] + 0x58 + 6] | keiempi[Offsets[Section] + 0x58 + 7] << 8);
            if (Section!=8)
                csv = csv_intro + "#" + SecMagic[1, Section] + "\n" + "#Amount of Points: " + numpt.ToString() + "\n";
            switch (Section)
            {
                //KTPT (Not handled)
                //ENPT
                case 1:
                    {
                        csv += "#X,Y,Z,Point Size,Unknown1,Unknown2\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            for (int ii = 2; ii < 6; ii++)
                            {
                                csv += BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + ii*4 + i * PointSizes[Section]) + ",";
                            }
                                csv += "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 24 + i * PointSizes[Section], 4).Replace("-", string.Empty) + ","
                                     + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 28 + i * PointSizes[Section], 4).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //ENPH
                case 2:
                    {
                        csv += "#Start,Length,Previous1,Previous2,Previous3,Previous4,Previous5,Previous6,Previous7,Previous8,Previous9,Previous10,Previous11,Previous12,Previous13,Previous14,Previous15,Previous16,Next1,Next2,Next3,Next4,Next5,Next6,Next7,Next8,Next9,Next10,Next11,Next12,Next13,Next14,Next15,Next16,Unknown\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            for (int ii=0; ii<34; ii++) {
                                csv += (short)(keiempi[Offsets[Section] + 0x58 + 8 + ii * 2 + i * PointSizes[Section]] | keiempi[Offsets[Section] + 0x58 + 8 + ii * 2 + 1 + i * PointSizes[Section]] << 8) + ",";
                            }
                                csv += "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 76 + i * PointSizes[Section], 4).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //ITPT
                case 3:
                    {
                        csv += "#X,Y,Z,Point Size,Unknown\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            for (int ii=0; i<4; i++) {
                                csv += BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 8 + ii*4 + i * PointSizes[Section]) + ",";
                            }
                            csv += "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 24 + i * PointSizes[Section], 4).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //ITPH
                case 4:
                    {
                        csv += "#Start,Length,Previous1,Previous2,Previous3,Previous4,Previous5,Previous6,Next1,Next2,Next3,Next4,Next5,Next6\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            for (int ii=0; ii<13; ii++) {
                                csv += (short)(keiempi[Offsets[Section] + 0x58 + 8 + ii*2 + i * PointSizes[Section]] | keiempi[Offsets[Section] + 0x58 + 8 + ii*2 + 1 + i * PointSizes[Section]] << 8) + ",";
                            }
                            csv += (short)(keiempi[Offsets[Section] + 0x58 + 34 + i * PointSizes[Section]] | keiempi[Offsets[Section] + 0x58 + 34 + 1 + i * PointSizes[Section]] << 8) + "\n";
                        }
                        break;
                    }
                //CKPT
                case 5:
                    {
                        csv += "#X1,Z1,X2,Z2,Respawn,Type,Previous,Next,Unkown1,SectionCount,Unknown2,Unknown3\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            csv += BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 8 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 12 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 16 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 20 + i * PointSizes[Section]) + ","
                                 + keiempi[Offsets[Section] + 0x58 + 24 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 25 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 26 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 27 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 28 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 29 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 30 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 31 + i * PointSizes[Section]] + "\n";
                        }
                        break;
                    }
                //CKPH
                case 6:
                    {
                        csv += "#Start,Length,Previous1,Previous2,Previous3,Previous4,Previous5,Previous6,Next1,Next2,Next3,Next4,Next5,Next6,Unknown\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            csv += keiempi[Offsets[Section] + 0x58 + 8 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 9 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 10 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 11 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 12 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 13 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 14 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 15 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 16 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 17 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 18 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 19 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 20 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 21 + i * PointSizes[Section]] + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 22 + i * PointSizes[Section], 2).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //GOBJ
                case 7:
                    {
                        csv += "#Object ID,Unknown0,X,Y,Z,X Angle,Y Angle,Z Angle,X Scale,Y Scale,Z Scale,Route ID,Setting 1,Setting 2,Setting 3,Setting 4,Setting 5,Setting 6,Setting 7,Setting 8,Visibility,Unknown1,Unknown2\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            csv += "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 8 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 10 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 12 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 16 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 20 + i * PointSizes[Section]) + ","
                                 + Rad2Deg(BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 24 + i * PointSizes[Section])) + ","
                                 + Rad2Deg(BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 28 + i * PointSizes[Section])) + ","
                                 + Rad2Deg(BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 32 + i * PointSizes[Section])) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 36 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 40 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 44 + i * PointSizes[Section]) + ","
                                 + (short)(keiempi[Offsets[Section] + 0x58 + 48 + i * PointSizes[Section]] | keiempi[Offsets[Section] + 0x58 + 49 + i * PointSizes[Section]] << 8) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 50 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 52 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 54 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 56 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 58 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 60 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 62 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 64 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 66 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 68 + i * PointSizes[Section], 2).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 70 + i * PointSizes[Section], 2).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //POTI
                case 8:
                    {
                        short[,] usBy = new short[numpt, 2];
                        
                        //Initialize usBy
                        for (int r = 0; r < numpt; r++)
                            for (int c = 0; c < 2; c++)
                                usBy[r, c] = -1;

                        UsedBy(ref usBy);
                        byte numpt_ins = 0;
                        int posit = Offsets[Section]+ 8 + 0x58;
                        csv = csv_intro + "#" + SecMagic[1, Section] + "\n" + "#Amount of Sections: " + numpt.ToString() + "\n" + "#Total Amount of Points: " + numpoti.ToString() + "\n#NOTE! '$' Is a reserved character that indicates Route's sections Settings. You MUST put this BEFORE the section's points!\n";
                        for (int i = 0; i < numpt;i++)
                        {
                            numpt_ins = keiempi.Data[posit];
                            csv += "\n#Routes Section ID: " + i.ToString();
                            if (usBy[i,1] == 0)
                                csv += "\n#Used by Object " + usBy[i,0];
                            csv += "\n#Setting1,Setting2,Setting3\n";
                            csv += "$" + (keiempi.Data[posit + 1] & 0xFF).ToString() + "," + (keiempi.Data[posit + 2] & 0xFF).ToString() + "," + (keiempi.Data[posit + 3] & 0xFF).ToString() + "\n";
                            csv += "#Amount of Points: " + numpt_ins.ToString() + "\n";
                            csv += "#X,Y,Z,Unknown\n";
                            posit += 4;
                            for (int p = 0; p < numpt_ins;p++,posit+=16)
                            {
                                csv += BitConverter.ToSingle(keiempi.Data, posit) + ","
                                    + BitConverter.ToSingle(keiempi.Data, posit + 4) + ","
                                    + BitConverter.ToSingle(keiempi.Data, posit + 8) + ","
                                    + "'" + BitConverter.ToString(keiempi.Data, posit + 12, 4).Replace("-", string.Empty) + "\n";
                            }
                        }
                        break;
                    }
                //JGPT
                case 11:
                    {
                        csv += "#X,Y,Z,X Angle,Y Angle,Z Angle,Index,Unknown\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            csv += BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 8 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 12 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 16 + i * PointSizes[Section]) + ","
                                 + Rad2Deg(BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 20 + i * PointSizes[Section])) + ","
                                 + Rad2Deg(BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 24 + i * PointSizes[Section])) + ","
                                 + Rad2Deg(BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 28 + i * PointSizes[Section])) + ","
                                 + (short)(keiempi[Offsets[Section] + 0x58 + 32 + i * PointSizes[Section]] | keiempi[Offsets[Section] + 0x58 + 33 + i * PointSizes[Section]] << 8) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 34 + i * PointSizes[Section], 2).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //GLPT
                case 16:
                    {
                        csv += "#X,Y,Z,Scale,Unknown1,Unknown2\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            csv += BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 8 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 12 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 16 + i * PointSizes[Section]) + ","
                                 + BitConverter.ToSingle(keiempi.Data, Offsets[Section] + 0x58 + 20 + i * PointSizes[Section]) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 24 + i * PointSizes[Section], 4).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 28 + i * PointSizes[Section], 4).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
                //GLPH
                case 17:
                    {
                        csv += "#Start,Length,Previous1,Previous2,Previous3,Previous4,Previous5,Previous6,Next1,Next2,Next3,Next4,Next5,Next6,Unknown1,Unknown2\n";
                        for (int i = 0; i < numpt; i++)
                        {
                            csv += keiempi[Offsets[Section] + 0x58 + 8 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 9 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 10 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 11 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 12 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 13 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 14 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 15 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 16 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 17 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 18 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 19 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 20 + i * PointSizes[Section]] + ","
                                 + keiempi[Offsets[Section] + 0x58 + 21 + i * PointSizes[Section]] + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 22 + i * PointSizes[Section], 4).Replace("-", string.Empty) + ","
                                 + "'" + BitConverter.ToString(keiempi.Data, Offsets[Section] + 0x58 + 26 + i * PointSizes[Section], 4).Replace("-", string.Empty) + "\n";
                        }
                        break;
                    }
            }
            saveCSV.ShowDialog();
        }


        void ParseCSV()
        {
            int Section = comboBox1.SelectedIndex;
            int k=0;
            int dollars = 0;
            var csv_lst = new List<string>();
            csv_lst.AddRange(csv_parse);
            for (; k < csv_lst.Count(); ) 
            {
                if ((csv_lst[k] == "") || (csv_lst[k].ToCharArray()[0] == '#'))
                    csv_lst.RemoveRange(k, 1);
                else
                    k++;
            }
            for (k = 0; k < csv_lst.Count();k++ )
            {
                if (csv_lst[k].ToCharArray()[0] == '$')
                    dollars++;
            }
            csv_parse = csv_lst.ToArray();
            short numpt = 0;
            short numpoti = 0;
            if (Section!=8)
                numpt = (short)(csv_parse.Length);
            else
            {
                numpt = (short)dollars;
                numpoti = (short)((csv_parse.Length) - dollars);
            }
            bool tf = true;
            int j = 0;
            int lines = csv_parse.Length;
            if (Section != 8)
                for (j = 0; (j < numpt && tf); j++)
                    tf = csv_parse[j].Split(',').Length == NumData[Section];
            else
            {
                for (j = 0; (j < lines && tf); j++)
                {
                    tf = ((csv_parse[j].Split(',').Length == NumData[Section]) || ((csv_parse[j].ToCharArray()[0] == '$') && (csv_parse[0].Split(',').Length>=3)));
                }
            }
            if (tf)
            {
                //Magic+NumPt
                int saizu = 0;
                if (Section != 8)
                    saizu = numpt * PointSizes[Section] + 8;
                else
                {
                    saizu = 8 + numpt * 4 + numpoti * 16;
                }
                byte[] SecData = new byte[saizu];
                SecData[0] = (byte)SecMagic[0, Section].ToCharArray()[0];
                SecData[1] = (byte)SecMagic[0, Section].ToCharArray()[1];
                SecData[2] = (byte)SecMagic[0, Section].ToCharArray()[2];
                SecData[3] = (byte)SecMagic[0, Section].ToCharArray()[3];
                SecData[4] = (byte)((int)numpt & 0xFF);
                SecData[5] = (byte)(((int)numpt >> 8) & 0xFF);
                SecData[6] = (byte)((int)numpoti & 0xFF);
                SecData[7] = (byte)(((int)numpoti >> 8) & 0xFF);
                //Serious stuff
                switch (Section)
                {
                    //ENPT
                    case 1:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8    , 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 4 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 8 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 12 + 8, 4);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[4]), 0, SecData, i * PointSizes[Section] + 16 + 8, 4);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[5]), 0, SecData, i * PointSizes[Section] + 20 + 8, 4);
                            }
                            break;
                        }
                    //ENPH
                    case 2:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 2 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 4 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 6 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[4])), 0, SecData, i * PointSizes[Section] + 8 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[5])), 0, SecData, i * PointSizes[Section] + 10 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[6])), 0, SecData, i * PointSizes[Section] + 12 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[7])), 0, SecData, i * PointSizes[Section] + 14 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[8])), 0, SecData, i * PointSizes[Section] + 16 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[9])), 0, SecData, i * PointSizes[Section] + 18 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[10])), 0, SecData, i * PointSizes[Section] + 20 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[11])), 0, SecData, i * PointSizes[Section] + 22 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[12])), 0, SecData, i * PointSizes[Section] + 24 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[13])), 0, SecData, i * PointSizes[Section] + 26 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[14])), 0, SecData, i * PointSizes[Section] + 28 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[15])), 0, SecData, i * PointSizes[Section] + 30 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[16])), 0, SecData, i * PointSizes[Section] + 32 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[17])), 0, SecData, i * PointSizes[Section] + 34 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[18])), 0, SecData, i * PointSizes[Section] + 36 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[19])), 0, SecData, i * PointSizes[Section] + 38 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[20])), 0, SecData, i * PointSizes[Section] + 40 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[21])), 0, SecData, i * PointSizes[Section] + 42 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[22])), 0, SecData, i * PointSizes[Section] + 44 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[23])), 0, SecData, i * PointSizes[Section] + 46 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[24])), 0, SecData, i * PointSizes[Section] + 48 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[25])), 0, SecData, i * PointSizes[Section] + 50 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[26])), 0, SecData, i * PointSizes[Section] + 52 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[27])), 0, SecData, i * PointSizes[Section] + 54 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[28])), 0, SecData, i * PointSizes[Section] + 56 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[29])), 0, SecData, i * PointSizes[Section] + 58 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[30])), 0, SecData, i * PointSizes[Section] + 60 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[31])), 0, SecData, i * PointSizes[Section] + 62 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[32])), 0, SecData, i * PointSizes[Section] + 64 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[33])), 0, SecData, i * PointSizes[Section] + 66 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[34]), 0, SecData, i * PointSizes[Section] + 68 + 8, 4);
                            }
                            break;
                        }
                    //ITPT
                    case 3:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 4 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 8 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 12 + 8, 4);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[4]), 0, SecData, i * PointSizes[Section] + 16 + 8, 4);
                            }
                            break;
                        }
                    //ITPH
                    case 4:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 2 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 4 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 6 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[4])), 0, SecData, i * PointSizes[Section] + 8 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[5])), 0, SecData, i * PointSizes[Section] + 10 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[6])), 0, SecData, i * PointSizes[Section] + 12 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[7])), 0, SecData, i * PointSizes[Section] + 14 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[8])), 0, SecData, i * PointSizes[Section] + 16 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[9])), 0, SecData, i * PointSizes[Section] + 18 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[10])), 0, SecData, i * PointSizes[Section] + 20 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[11])), 0, SecData, i * PointSizes[Section] + 22 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[12])), 0, SecData, i * PointSizes[Section] + 24 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[13])), 0, SecData, i * PointSizes[Section] + 26 + 8, 2);
                            }
                            break;
                        }
                    //CKPT
                    case 5:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 4 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 8 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 12 + 8, 4);
                                SecData[i * PointSizes[Section] + 16 + 8] = byte.Parse(csv_parse[i].Split(',')[4]);
                                SecData[i * PointSizes[Section] + 17 + 8] = byte.Parse(csv_parse[i].Split(',')[5]);
                                SecData[i * PointSizes[Section] + 18 + 8] = byte.Parse(csv_parse[i].Split(',')[6]);
                                SecData[i * PointSizes[Section] + 19 + 8] = byte.Parse(csv_parse[i].Split(',')[7]);
                                SecData[i * PointSizes[Section] + 20 + 8] = byte.Parse(csv_parse[i].Split(',')[8]);
                                SecData[i * PointSizes[Section] + 21 + 8] = byte.Parse(csv_parse[i].Split(',')[9]);
                                SecData[i * PointSizes[Section] + 22 + 8] = byte.Parse(csv_parse[i].Split(',')[10]);
                                SecData[i * PointSizes[Section] + 23 + 8] = byte.Parse(csv_parse[i].Split(',')[11]);
                            }
                            break;
                        }
                    //CKPH
                    case 6:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                SecData[i * PointSizes[Section] + 8] = byte.Parse(csv_parse[i].Split(',')[0]);
                                SecData[i * PointSizes[Section] + 1 + 8] = byte.Parse(csv_parse[i].Split(',')[1]);
                                SecData[i * PointSizes[Section] + 2 + 8] = byte.Parse(csv_parse[i].Split(',')[2]);
                                SecData[i * PointSizes[Section] + 3 + 8] = byte.Parse(csv_parse[i].Split(',')[3]);
                                SecData[i * PointSizes[Section] + 4 + 8] = byte.Parse(csv_parse[i].Split(',')[4]);
                                SecData[i * PointSizes[Section] + 5 + 8] = byte.Parse(csv_parse[i].Split(',')[5]);
                                SecData[i * PointSizes[Section] + 6 + 8] = byte.Parse(csv_parse[i].Split(',')[6]);
                                SecData[i * PointSizes[Section] + 7 + 8] = byte.Parse(csv_parse[i].Split(',')[7]);
                                SecData[i * PointSizes[Section] + 8 + 8] = byte.Parse(csv_parse[i].Split(',')[8]);
                                SecData[i * PointSizes[Section] + 9 + 8] = byte.Parse(csv_parse[i].Split(',')[9]);
                                SecData[i * PointSizes[Section] + 10 + 8] = byte.Parse(csv_parse[i].Split(',')[10]);
                                SecData[i * PointSizes[Section] + 11 + 8] = byte.Parse(csv_parse[i].Split(',')[11]);
                                SecData[i * PointSizes[Section] + 12 + 8] = byte.Parse(csv_parse[i].Split(',')[12]);
                                SecData[i * PointSizes[Section] + 13 + 8] = byte.Parse(csv_parse[i].Split(',')[13]);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[14]), 0, SecData, i * PointSizes[Section] + 14 + 8, 2);
                            }
                            break;
                        }
                    //GOBJ
                    case 7:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[0]), 0, SecData, i * PointSizes[Section] + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[1]), 0, SecData, i * PointSizes[Section] + 2 + 8, 2);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 4 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 8 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[4])), 0, SecData, i * PointSizes[Section] + 12 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(Deg2Rad(float.Parse(csv_parse[i].Split(',')[5]))), 0, SecData, i * PointSizes[Section] + 16 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(Deg2Rad(float.Parse(csv_parse[i].Split(',')[6]))), 0, SecData, i * PointSizes[Section] + 20 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(Deg2Rad(float.Parse(csv_parse[i].Split(',')[7]))), 0, SecData, i * PointSizes[Section] + 24 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[8])), 0, SecData, i * PointSizes[Section] + 28 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[9])), 0, SecData, i * PointSizes[Section] + 32 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[10])), 0, SecData, i * PointSizes[Section] + 36 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[11])), 0, SecData, i * PointSizes[Section] + 40 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[12]), 0, SecData, i * PointSizes[Section] + 42 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[13]), 0, SecData, i * PointSizes[Section] + 44 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[14]), 0, SecData, i * PointSizes[Section] + 46 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[15]), 0, SecData, i * PointSizes[Section] + 48 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[16]), 0, SecData, i * PointSizes[Section] + 50 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[17]), 0, SecData, i * PointSizes[Section] + 52 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[18]), 0, SecData, i * PointSizes[Section] + 54 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[19]), 0, SecData, i * PointSizes[Section] + 56 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[20]), 0, SecData, i * PointSizes[Section] + 58 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[21]), 0, SecData, i * PointSizes[Section] + 60 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[22]), 0, SecData, i * PointSizes[Section] + 62 + 8, 2);
                            }
                            break;
                        }
                    //POTI
                    case 8:
                        {
                            
                            int posit = 8;
                            int prev = 0;
                            int numpt_ins = 0;
                            for (int i = 0; i < lines; i++)
                            {
                                if (csv_parse[i].ToCharArray()[0] == '$')
                                {
                                    if (prev!=0) SecData[prev]=(byte)numpt_ins;
                                    SecData[posit+1]=byte.Parse(csv_parse[i].Split(',')[0].Replace("$",string.Empty));
                                    SecData[posit+2]=byte.Parse(csv_parse[i].Split(',')[1]);
                                    SecData[posit+3]=byte.Parse(csv_parse[i].Split(',')[2]);
                                    numpt_ins=0;
                                    prev=posit;
                                    posit+=4;
                                }
                                else
                                {
                                    numpt_ins++;
                                    Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[0])), 0, SecData, posit, 4);
                                    Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[1])), 0, SecData, posit + 4, 4);
                                    Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, posit + 8, 4);
                                    Array.Copy(StringToByte(csv_parse[i].Split(',')[3]), 0, SecData, posit + 12, 4);
                                    posit+=16;
                                }
                            }
                            SecData[prev]=(byte)numpt_ins;
                            break;
                        }
                    //JGPT
                    case 11:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 4 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 8 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(Deg2Rad(float.Parse(csv_parse[i].Split(',')[3]))), 0, SecData, i * PointSizes[Section] + 12 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(Deg2Rad(float.Parse(csv_parse[i].Split(',')[4]))), 0, SecData, i * PointSizes[Section] + 16 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(Deg2Rad(float.Parse(csv_parse[i].Split(',')[5]))), 0, SecData, i * PointSizes[Section] + 20 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(short.Parse(csv_parse[i].Split(',')[6])), 0, SecData, i * PointSizes[Section] + 24 + 8, 2);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[7]), 0, SecData, i * PointSizes[Section] + 26 + 8, 2);
                            }
                            break;
                        }
                    //GLPT
                    case 16:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[0])), 0, SecData, i * PointSizes[Section] + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[1])), 0, SecData, i * PointSizes[Section] + 4 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[2])), 0, SecData, i * PointSizes[Section] + 8 + 8, 4);
                                Array.Copy(BitConverter.GetBytes(float.Parse(csv_parse[i].Split(',')[3])), 0, SecData, i * PointSizes[Section] + 12 + 8, 4);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[4]), 0, SecData, i * PointSizes[Section] + 16 + 8, 4);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[5]), 0, SecData, i * PointSizes[Section] + 20 + 8, 4);
                            }
                            break;
                        }
                    //GLPH
                    case 17:
                        {
                            for (int i = 0; i < numpt; i++)
                            {
                                SecData[i * PointSizes[Section] + 8] = byte.Parse(csv_parse[i].Split(',')[0]);
                                SecData[i * PointSizes[Section] + 1 + 8] = byte.Parse(csv_parse[i].Split(',')[1]);
                                SecData[i * PointSizes[Section] + 2 + 8] = byte.Parse(csv_parse[i].Split(',')[2]);
                                SecData[i * PointSizes[Section] + 3 + 8] = byte.Parse(csv_parse[i].Split(',')[3]);
                                SecData[i * PointSizes[Section] + 4 + 8] = byte.Parse(csv_parse[i].Split(',')[4]);
                                SecData[i * PointSizes[Section] + 5 + 8] = byte.Parse(csv_parse[i].Split(',')[5]);
                                SecData[i * PointSizes[Section] + 6 + 8] = byte.Parse(csv_parse[i].Split(',')[6]);
                                SecData[i * PointSizes[Section] + 7 + 8] = byte.Parse(csv_parse[i].Split(',')[7]);
                                SecData[i * PointSizes[Section] + 8 + 8] = byte.Parse(csv_parse[i].Split(',')[8]);
                                SecData[i * PointSizes[Section] + 9 + 8] = byte.Parse(csv_parse[i].Split(',')[9]);
                                SecData[i * PointSizes[Section] + 10 + 8] = byte.Parse(csv_parse[i].Split(',')[10]);
                                SecData[i * PointSizes[Section] + 11 + 8] = byte.Parse(csv_parse[i].Split(',')[11]);
                                SecData[i * PointSizes[Section] + 12 + 8] = byte.Parse(csv_parse[i].Split(',')[12]);
                                SecData[i * PointSizes[Section] + 13 + 8] = byte.Parse(csv_parse[i].Split(',')[13]);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[14]), 0, SecData, i * PointSizes[Section] + 14 + 8, 4);
                                Array.Copy(StringToByte(csv_parse[i].Split(',')[15]), 0, SecData, i * PointSizes[Section] + 18 + 8, 4);
                            }
                            break;
                        }
                }
                Inject(SecData);
                MessageBox.Show(SecMagic[1, Section] + " injected successfully!");
            }
            else 
                if (Section!=8) MessageBox.Show("Computable Line "+(j)+" contains less fields than expected in a "+SecMagic[1,Section]+" Section.\nThe number of fields should be "+NumData[Section]+".", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Computable Line " + (j) + " contains less fields than expected in a " + SecMagic[1, Section] + " Section.\nThe number of fields should be " + NumData[Section] + " or if it's a Section header (The ones followed by '$'), it must have at least 3 fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /*
          */

        public KMPEd()
        {
            InitializeComponent();
        }

        private void openKMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openKMP.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string path = openKMP.FileName;
                try
                {
                    keiempi = new KMP(File.ReadAllBytes(path));
                    SizeOfKMP = keiempi.Data.Length;
                    CheckHeader();
                    checkSectPoint();
                }
                catch (IOException)
                {
                    MessageBox.Show("Error when reading from file!");
                }
            }
        }

        private void openKMP_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string name = saveKMP.FileName;
            File.Create(name).Close();
            File.WriteAllBytes(name, keiempi.Data);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveKMP.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            extractbin.Enabled=true;
            injectbin.Enabled=true;
            if ((PointSizes[comboBox1.SelectedIndex] == 255) && (comboBox1.SelectedIndex!=8))
            {
                extractcsv.Enabled = false;
                injectcsv.Enabled = false;
            }
            else
            {
                extractcsv.Enabled = true;
                injectcsv.Enabled = true;
            }

        }

        private void extractbin_Click(object sender, EventArgs e)
        {
            saveBinary.ShowDialog();
        }

        private void injectbin_Click(object sender, EventArgs e)
        {
            DialogResult result = openBinary.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string path = openBinary.FileName;
                try
                {
                    bin = new KMP(File.ReadAllBytes(path));
                    ImportBin();
                }
                catch (IOException)
                {
                    MessageBox.Show("Error when reading from file!");
                }
            }
        }

        private void extractcsv_Click(object sender, EventArgs e)
        {
            ExportCSV();
        }

        private void injectcsv_Click(object sender, EventArgs e)
        {
            DialogResult result = openCSV.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string path = openCSV.FileName;
                try
                {
                    csv_parse = File.ReadAllLines(path);
                    ParseCSV();
                }
                catch (IOException)
                {
                    MessageBox.Show("Error when reading from file!");
                }
            }
        }

        private void saveBinary_FileOk(object sender, CancelEventArgs e)
        {
            int size = 0;
            if (comboBox1.SelectedIndex == 17)
                size = SizeOfKMP - (Offsets[comboBox1.SelectedIndex] + 0x58);
            else size = (Offsets[comboBox1.SelectedIndex + 1] - Offsets[comboBox1.SelectedIndex]);
            byte[] binary = new byte[size];
            Array.Copy(keiempi.Data, (Offsets[comboBox1.SelectedIndex] + 0x58), binary, 0, binary.Length);
            string name = saveBinary.FileName;
            File.Create(name).Close();
            File.WriteAllBytes(name, binary);
        }

        private void filesize_box_TextChanged(object sender, EventArgs e)
        {

        }

        private void saveCSV_FileOk(object sender, CancelEventArgs e)
        {
            string name = saveCSV.FileName;
            File.Create(name).Close();
            File.WriteAllText(name, csv);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("KMP Expander (" + Ver +") - made by Ermelber\n\nThis tool is capable of editing KMP files by injecting binary or comma separated values (in combination with any Spreadsheet software).\n\nSpecial Thanks to Gericom for some programming tips.\n\n(c) 2015 Ermelber");
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        } 
    }
}
