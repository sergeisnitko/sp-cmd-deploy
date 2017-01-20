using System;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;

public class SpSimpleAES
{
    private byte[] Key = { };
    private byte[] Vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 221, 112, 79, 32, 114, 156 };


    private ICryptoTransform EncryptorTransform, DecryptorTransform;
    private System.Text.UTF8Encoding UTFEncoder;

    public SpSimpleAES()
    {
        this.Key = Encoding.ASCII.GetBytes(this.GetMachineGuid());
        RijndaelManaged rm = new RijndaelManaged();

        EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
        DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

        UTFEncoder = new System.Text.UTF8Encoding();
    }

    public string GetMaxMachineGuid()
    {
        var res = this.GetMachineGuid();
        for (var i = 0; i < 2; i++)
        {
            res += res;
        }
        return res;
    }
    public string GetMachineGuid()
    {
        string location = @"SOFTWARE\Microsoft\Cryptography";
        string name = "MachineGuid";

        using (RegistryKey localMachineX64View =
            RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
        {
            using (RegistryKey rk = localMachineX64View.OpenSubKey(location))
            {
                if (rk == null)
                    throw new KeyNotFoundException(
                        string.Format("Key Not Found: {0}", location));

                object machineGuid = rk.GetValue(name);
                if (machineGuid == null)
                    throw new IndexOutOfRangeException(
                        string.Format("Index Not Found: {0}", name));

                return machineGuid.ToString().Replace("-", "");
            }
        }
    }

    public string EncryptToString(string TextValue)
    {
        return ByteArrToString(Encrypt(TextValue));
    }

    public byte[] Encrypt(string TextValue)
    {
        Byte[] bytes = UTFEncoder.GetBytes(TextValue);
        MemoryStream memoryStream = new MemoryStream();

        #region Write the decrypted value to the encryption stream
        CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
        cs.Write(bytes, 0, bytes.Length);
        cs.FlushFinalBlock();
        #endregion

        #region Read encrypted value back out of the stream
        memoryStream.Position = 0;
        byte[] encrypted = new byte[memoryStream.Length];
        memoryStream.Read(encrypted, 0, encrypted.Length);
        #endregion

        cs.Close();
        memoryStream.Close();

        return encrypted;
    }

    public string DecryptString(string EncryptedString)
    {
        try {
            return Decrypt(StrToByteArray(EncryptedString));
        }
        catch
        {
            return EncryptedString;
        }
    }

    public string Decrypt(byte[] EncryptedValue)
    {
        #region Write the encrypted value to the decryption stream
        MemoryStream encryptedStream = new MemoryStream();
        CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
        decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
        decryptStream.FlushFinalBlock();
        #endregion

        #region Read the decrypted value from the stream.
        encryptedStream.Position = 0;
        Byte[] decryptedBytes = new Byte[encryptedStream.Length];
        encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
        encryptedStream.Close();
        #endregion
        return UTFEncoder.GetString(decryptedBytes);
    }
    public byte[] StrToByteArray(string str)
    {
        if (str.Length == 0)
            throw new Exception("Invalid string value in StrToByteArray");

        byte val;
        byte[] byteArr = new byte[str.Length / 3];
        int i = 0;
        int j = 0;
        do
        {
            val = byte.Parse(str.Substring(i, 3));
            byteArr[j++] = val;
            i += 3;
        }
        while (i < str.Length);
        return byteArr;
    }

    public string ByteArrToString(byte[] byteArr)
    {
        byte val;
        string tempStr = "";
        for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
        {
            val = byteArr[i];
            if (val < (byte)10)
                tempStr += "00" + val.ToString();
            else if (val < (byte)100)
                tempStr += "0" + val.ToString();
            else
                tempStr += val.ToString();
        }
        return tempStr;
    }
}