﻿//******************************************************************************
//Ultima Underworld Save Editor - Shane Harrison
//Documentation used:
//   ftp://files.chatnfiles.com/Infomagic-Windows/Infomagic-Games-for-Daze-Summer-95-1/EDITORS/UWEDITOR/UWEDITOR.TXT
//   http://bootstrike.com/Ultima/Online/uwformat.php
//
//******************************************************************************

//Some notes
//Address 0x3B seems to store the value which determines the avatars fatigue (awake, drowsy, fatigue, etc) 0x00 - 0x79 = "fatigued" ::::  0x80 - 0x9F = "awake" :::: 0xA0 - 0xAF "wide awake" :::: 0xB0 - 0xB7 "rested" :::: 0xB8 - 0xBF "wide awake" :::: 0xC0 - 0xCF "very tired" :::: 0xD0 - 0xDF  "fatigued" :::: 0xE0 - 0xFF "drowsy"
//Addresses 0x4B - 0x4E (4 bytes, 32 bit int?) seem to change values of max weight the avatar can carry. (maybe not 0x4E....test)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UW_Save_Editor
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd = new OpenFileDialog();
        FileStream fs;
        bool fileOK = false;
        int[] XORterms;

        public Form1()
        {
            InitializeComponent();
        }

        private void changeStatsButton_Click(object sender, EventArgs e)
        {
            //Grab all the values from our user inputs
            int strength = int.Parse(strBox.Text);
            int dexterity = int.Parse(dexBox.Text);
            int intelligence = int.Parse(intBox.Text);
            int attack = int.Parse(atkBox.Text);
            int defense = int.Parse(defBox.Text);
            int unarmed = int.Parse(unarmedBox.Text);
            int sword = int.Parse(swrdBox.Text);
            int axe = int.Parse(axeBox.Text);
            int mace = int.Parse(maceBox.Text);
            int missile = int.Parse(mslBox.Text);
            int mana = int.Parse(manaStatBox.Text);
            int lore = int.Parse(loreBox.Text);
            int casting = int.Parse(castBox.Text);
            int traps = int.Parse(trapBox.Text);
            int search = int.Parse(searchBox.Text);
            int track = int.Parse(trackBox.Text);
            int sneak = int.Parse(sneakBox.Text);
            int repair = int.Parse(repairBox.Text);
            int charm = int.Parse(charmBox.Text);
            int picklock = int.Parse(pickBox.Text);
            int acrobat = int.Parse(acroBox.Text);
            int appraise = int.Parse(appBox.Text);
            int swimming = int.Parse(swimBox.Text);

            const byte strengthOffset = 0x1F;

            const int NAME_LENGTH = 15;
            const int NO_OF_BYTES = 200; //From 0x01 to 0x5D (where 0x5D stores current floor/level)
            int startXOR;
            int xorIncrement = 0x03;
            int statLevelWanted;
            char nameCharacter;
            string nameString = "";
            byte byteConvered;


            //Array that holds all the user input fields
            int[] stats = {strength, dexterity, intelligence, attack, defense, unarmed, sword, axe, mace, missile, mana, lore, casting, traps, search, track, sneak, repair,
                           charm, picklock, acrobat, appraise, swimming};

            //Encoded bytes read before XOR calculations
            int[] encodedBytes = new int[NO_OF_BYTES];

            //Will store our values after undoing the XOR "encryption"
            int[] decodedValues = new int[NO_OF_BYTES];

            //All the XOR terms for each byte
            XORterms = new int[NO_OF_BYTES];

            //If file open and correct format
            if (fileOK == true)
            {
                fs = new FileStream(ofd.FileName, FileMode.Open);

                fs.Position = 0x00;

                //Will read offset 0x00 (this will be our starting XOR value)
                startXOR = fs.ReadByte();

                //Read all the bytes between 0x01 (The first character of name) and 0x35 (Value which determines swimming)
                for (int i = 0; i < encodedBytes.Length; i++)
                {
                    encodedBytes[i] = fs.ReadByte();
                }

                for (int i = 0; i < decodedValues.Length; i++)
                {
                    decodedValues[i] = encodedBytes[i] ^ (startXOR + xorIncrement);
                    xorIncrement += 3;

                    //If the Starting XOR + our increment goes over 0xFF, check its value and set our values back accordingly
                    if (xorIncrement + startXOR > 0xFF)
                    {
                        if (xorIncrement + startXOR == 0x100)
                        {
                            startXOR = 0x00;
                        }

                        if (xorIncrement + startXOR == 0x101)
                        {
                            startXOR = 0x01;
                        }

                        if (xorIncrement + startXOR == 0x102)
                        {
                            startXOR = 0x02;
                        }

                        xorIncrement = 0;
                    }
                }

                //Get players name and store it as a string in "nameString"
                for (int i = 0; i < NAME_LENGTH; i++)
                {
                    nameCharacter = (char)decodedValues[i];
                    nameString += nameCharacter.ToString();
                }

                //Show characters name on UI
                playerNameLabel.Text = "Player name: " + nameString;

                //Calculate all our XOR terms 
                for (int i = 0; i < XORterms.Length; i++)
                {
                    XORterms[i] = decodedValues[i] ^ encodedBytes[i];
                }

                //Set our position to strength byte offset (starting offset for all stats)
                fs.Position = strengthOffset;

                //Loop through all bytes between strength and swimming, calculate and set value accordingly
                for (int i = 0; i < stats.Length; i++)
                {
                    //i + 30 because strength starts at offset 31 (relative to length between offset 0x01 and 0x1F) and arrays start at [0]
                    statLevelWanted = stats[i] ^ XORterms[i + 30];
                    byteConvered = Convert.ToByte(statLevelWanted);
                    fs.WriteByte(byteConvered);
                }


                //offset 56 before offset 55 because little endian
                  MessageBox.Show("X position: " + decodedValues[85].ToString() + "." + decodedValues[84].ToString());
                  MessageBox.Show("Y position: " + decodedValues[87].ToString() + "." + decodedValues[86].ToString());
                  MessageBox.Show("Z position: " + decodedValues[89].ToString() + "." + decodedValues[88].ToString());



                /* byte xPos_1 = Convert.ToByte(200 ^ XORterms[84]);
                 byte xPos_2 = Convert.ToByte(208 ^ XORterms[85]);

                 byte yPos_1 = Convert.ToByte(65 ^ XORterms[86]);
                 byte yPos_2 = Convert.ToByte(18 ^ XORterms[87]);

                 byte zPos_1 = Convert.ToByte(16 ^ XORterms[88]);
                 byte zPos_2 = Convert.ToByte(19 ^ XORterms[89]);


                 fs.Position = positionOffset;

                 fs.WriteByte(xPos_1);
                 fs.WriteByte(xPos_2);

                 fs.WriteByte(yPos_1);
                 fs.WriteByte(yPos_2);

                 fs.WriteByte(zPos_1);
                 fs.WriteByte(zPos_2);
                */

                fs.Close();

                MessageBox.Show("Stats updated!");
            }

            else
            {
                MessageBox.Show("Please select a valid file.");
            }
        }


        private void openFileButton_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //Check if correct file extension 
                string filePath = ofd.FileName;
                string fileName = Path.GetFileName(filePath);
                string fileExtension = fileName.GetLast(4);

                if (fileExtension != ".DAT")
                {
                    fileOK = false;
                    MessageBox.Show("Incorrect file type.");
                }

                else
                {
                    fileOK = true;
                }
            }
        }

        //Teleportation still under construction
        private void teleportButton_Click(object sender, EventArgs e)
        {
            const byte positionOffset = 0x55;
            //byte xPos_1 = 0;
            //byte xPos_2 = 0;
            //byte yPos_1 = 0;
           // byte yPos_2 = 0;
            //byte zPos_1 = 0;
            //byte zPos_2 = 0;

            if (fileOK == true)
            {
                fs = new FileStream(ofd.FileName, FileMode.Open);
                fs.Position = positionOffset;

                if (startRadio.Checked)
                {
                    //Start coordinates = X = 208.200    Y = 18.65  Z = 19.16 (Keep in mind: little endian!)
                    byte xPos_1 = Convert.ToByte(200 ^ XORterms[84]);
                    byte xPos_2 = Convert.ToByte(208 ^ XORterms[85]);

                    byte yPos_1 = Convert.ToByte(65 ^ XORterms[86]);
                    byte yPos_2 = Convert.ToByte(18 ^ XORterms[87]);

                    byte zPos_1 = Convert.ToByte(16 ^ XORterms[88]);
                    byte zPos_2 = Convert.ToByte(19 ^ XORterms[89]);



                    fs.WriteByte(xPos_1);
                    fs.WriteByte(xPos_2);

                    fs.WriteByte(yPos_1);
                    fs.WriteByte(yPos_2);

                    fs.WriteByte(zPos_1);
                    fs.WriteByte(zPos_2);
                }

            }

            MessageBox.Show("Coordinates updated!");
            fs.Close();
        }

    }



    public static class StringExtension
    {
        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }
}
