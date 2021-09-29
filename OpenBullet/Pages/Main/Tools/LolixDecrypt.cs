using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace OpenBullet.Pages.Main.Tools
{
    // This was made by LethalLuck, uses the keys from Forlax's Modded OpenBullet.
    public partial class LolixDecrypt : Page
    {
        private static Random randomY = new Random();
        private string save;

        public LolixDecrypt()
        {
            InitializeComponent();
            FileName = "";
        }

        public static string FileName;

        private void LoadFromManagerButton_Click(object sender, RoutedEventArgs e)
        {
            new OpenFileDialog();
        }

        
        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog lolixConfig = new OpenFileDialog();
            if (lolixConfig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string strfilename = lolixConfig.FileName;
                FileName = lolixConfig.FileName;
                PathName.Text = FileName;
            }
            if (FileName != "")
            {
                string s = File.ReadAllText(PathName.Text);
                try
                {
                    if (!s.Contains("Body") || !s.Contains("ID"))
                    {
                        byte[] bytes = Convert.FromBase64String(s);
                        s = DecryptX(Regex.Match(Encoding.UTF8.GetString(bytes), "x0;(.*?)0;x").Groups[1].Value, "0THISISOBmodedByForlaxNIGGAs");
                        textBox1.Text = s;
                        save = s;
                    }
                    else
                    {
                        byte[] bytes = Convert.FromBase64String(BellaCiao(Regex.Match(s, "\"Body\": \"(.*?)\"").Groups[1].Value, 2));
                        s = DecryptX(Regex.Match(Encoding.UTF8.GetString(bytes), "x0;(.*?)0;x").Groups[1].Value, "0THISISOBmodedByForlaxNIGGAs");
                        textBox1.Text = s;
                        save = s;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        
        private static string DecryptX(string cipherText, string passPhrase)
        {
            string str;
            byte[] source = Convert.FromBase64String(cipherText);
            byte[] salt = source.Take<byte>(32).ToArray<byte>();
            byte[] rgbIV = source.Skip<byte>(32).Take<byte>(32).ToArray<byte>();
            byte[] buffer = source.Skip<byte>(64).Take<byte>(source.Length - 64).ToArray<byte>();
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(passPhrase, salt, 1000))
            {
                byte[] rgbKey = bytes.GetBytes(32);
                using (RijndaelManaged managed = new RijndaelManaged())
                {
                    managed.BlockSize = 256;
                    managed.Mode = CipherMode.CBC;
                    managed.Padding = PaddingMode.None;
                    using (ICryptoTransform transform = managed.CreateDecryptor(rgbKey, rgbIV))
                    {
                        using (MemoryStream stream = new MemoryStream(buffer))
                        {
                            using (CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                            {
                                byte[] buffer6 = new byte[buffer.Length];
                                //stream.Close();
                                //stream2.Close();
                                str = Encoding.UTF8.GetString(buffer6, 0, stream2.Read(buffer6, 0, buffer6.Length));
                                stream2.Close();
                            }
                            stream.Close();
                        }
                    }
                }
            }
            return str;
        }

        private const int Keysize = 256;
        private const int DerivationIterations = 1000;

        
        public static string BellaCiao(string helpme, int op)
        {
            if (op != 1)
            {
                string str2 = "ay$a5%&jwrtmnh;lasjdf98787OMGFORLAX";
                string str3 = "abc@98797hjkas$&asd(*$%GJMANIGE";
                byte[] buffer = new byte[0];
                buffer = Encoding.UTF8.GetBytes(str3.Substring(0, 8));
                byte[] buffer2 = new byte[0];
                buffer2 = Encoding.UTF8.GetBytes(str2.Substring(0, 8));
                byte[] buffer3 = new byte[helpme.Length / 2];
                for (int i = 0; i < buffer3.Length; i++)
                {
                    buffer3[i] = Convert.ToByte(helpme.Substring(i * 2, 2), 0x10);
                }
                helpme = Encoding.UTF8.GetString(buffer3);
                byte[] buffer4 = new byte[helpme.Replace(" ", "+").Length];
                buffer4 = Convert.FromBase64String(helpme.Replace(" ", "+"));
                using (DESCryptoServiceProvider provider2 = new DESCryptoServiceProvider())
                {
                    MemoryStream stream = new MemoryStream();
                    CryptoStream stream3 = new CryptoStream(stream, provider2.CreateDecryptor(buffer2, buffer), CryptoStreamMode.Write);
                    stream3.Write(buffer4, 0, buffer4.Length);
                    stream3.FlushFinalBlock();
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            string s = "";
            string str5 = "ay$a5%&jwrtmnh;lasjdf98787OMGFORLAX";
            string str6 = "abc@98797hjkas$&asd(*$%GJMANIGE";
            byte[] rgbIV = new byte[0];
            rgbIV = Encoding.UTF8.GetBytes(str6.Substring(0, 8));
            byte[] rgbKey = new byte[0];
            rgbKey = Encoding.UTF8.GetBytes(str5.Substring(0, 8));
            byte[] bytes = Encoding.UTF8.GetBytes(helpme);
            using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
            {
                CryptoStream stream1 = new CryptoStream(new MemoryStream(), provider.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                stream1.Write(bytes, 0, bytes.Length);
                stream1.FlushFinalBlock();
                MemoryStream stream2 = new MemoryStream();
                s = Convert.ToBase64String(stream2.ToArray());
                StringBuilder builder = new StringBuilder();
                byte[] buffer8 = Encoding.UTF8.GetBytes(s);
                int index = 0;
                while (true)
                {
                    if (index >= buffer8.Length)
                    {
                        s = builder.ToString();
                        break;
                    }
                    builder.Append(buffer8[index].ToString("X2"));
                    index++;
                }
            }
            return s;
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(PathName.Text + "_Decrypted.anom", save);
            System.Windows.Forms.MessageBox.Show("Saved to: " + PathName.Text + "_Decrypted.anom");
        }
    }
}