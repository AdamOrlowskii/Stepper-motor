using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FTD2XX_NET;

namespace silnik_krokowy
{
    public partial class Form1 : Form
    {
        private FTDI silnik = new FTDI();

        private int index4 = 0;
        private int index8 = 0;
        private byte[] waveStepSequenceLeft = { 0x04, 0x02, 0x08, 0x01 };                   // sekwencja falowa
        private byte[] waveStepSequenceRight = { 0x01, 0x08, 0x02, 0x04 };
        private byte[] fullStepSequenceLeft = { 0x06, 0x0A, 0x09, 0x05 };                   // sekwencja pelnokrokowa
        private byte[] fullStepSequenceRight = { 0x05, 0x09, 0x0A, 0x06 };
        private byte[] halfStepSequenceRight = { 0x05, 0x01, 0x09, 0x08, 0x0A, 0x02, 0x06, 0x04 }; // sekwencja polkrokowa
        private byte[] halfStepSequenceLeft = { 0x04, 0x06, 0x02, 0x0A, 0x08, 0x09, 0x01, 0x05 };

        public Form1()
        {
            InitializeComponent();

            comboBox.Items.Add("falowe");
            comboBox.Items.Add("pełnokrokowe");
            comboBox.Items.Add("półkrokowe");
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            FTDI.FT_STATUS ftStatus = silnik.OpenByIndex(0);
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                richTextBox.AppendText("Polaczono z urzadzeniem.\r\n");
                silnik.SetBaudRate(9600);
                silnik.SetBitMode(0xFF, 1); // ustawienie trybu bitowego
            }
            else
            {
                richTextBox.AppendText("Proba polaczenia zakonczona niepowodzeniem.\r\n");
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            silnik.SetBitMode(0x00, 0);
            silnik.Close();
            richTextBox.AppendText("Odlaczono urzadzenie.\r\n");
        }

        private void leftButton_Click(object sender, EventArgs e)
        {
            MoveMotor(-1);
        }

        private void rightButton_Click(object sender, EventArgs e)
        {
            MoveMotor(1);
        }

        private void MoveMotor(int direction)
        {
            if (!int.TryParse(nTextBox.Text, out int steps) || !int.TryParse(timeTextBox.Text, out int speed))
            {
                richTextBox.AppendText("Bledne dane wejsciowe.\r\n");
                return;
            }

            string mode = comboBox.SelectedItem?.ToString() ?? "pełnokrokowe";

            switch (mode)
            {
                case "pełnokrokowe":
                    FullStep(steps, direction, speed);
                    break;
                case "półkrokowe":
                    HalfStep(steps, direction, speed);
                    break;
                case "falowe":
                    WaveStep(steps, direction, speed);
                    break;
                default:
                    richTextBox.AppendText("Bledny tryb pracy.\r\n");
                    break;
            }
        }

        private void FullStep(int steps, int direction, int speed)
        {
            byte[] sequence = direction > 0 ? fullStepSequenceRight : fullStepSequenceLeft;
            ExecuteSteps(sequence, steps, speed, 4);
        }

        private void HalfStep(int steps, int direction, int speed)
        {
            byte[] sequence = direction > 0 ? halfStepSequenceRight : halfStepSequenceLeft;
            ExecuteSteps(sequence, steps, speed, 8);
        }

        private void WaveStep(int steps, int direction, int speed)
        {
            byte[] sequence = direction > 0 ? waveStepSequenceRight : waveStepSequenceLeft;
            ExecuteSteps(sequence, steps, speed, 4);
        }

        private void ExecuteSteps(byte[] sequence, int steps, int speed, int length)
        {
            for (int i = 0; i < steps; i++)
            {
                uint BytesWritten = 0;
                if (length == 4)
                {
                    silnik.Write(new byte[] { sequence[index4] }, 1, ref BytesWritten);
                    index4++;
                    index4 %= 4;
                } else
                {
                    silnik.Write(new byte[] { sequence[index8] }, 1, ref BytesWritten);
                    index8++;
                    index8 %= 8;
                }
                
                System.Threading.Thread.Sleep(speed);
            }
            richTextBox.AppendText($"Wykonano {steps} krokow.\r\n");
        }
        private byte[] ReverseArray(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }
    }
}
