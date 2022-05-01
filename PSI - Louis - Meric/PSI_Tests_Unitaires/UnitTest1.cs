using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PSI___Louis___Meric
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestConvertCharToAlphanum()
        {
            char h = 'H';
            int alphaNum = MyImage.ConvertCharToAlphanum(h);
            bool validé = false;
            int attendu = 17;
            if (alphaNum == attendu)
                validé = true;
            Assert.AreEqual(validé, true);
        }

        [TestMethod]

        public void TestConvert_Int_To_Endian()
        {
            int entier = 1948;
            byte[] test = MyImage.Convert_Int_To_Endian(entier);
            byte[] attendu = { 156, 7, 0, 0 };
            bool validé = true;
            for (int i = 0; i < attendu.Length; i++)
                if (test[i] != attendu[i]) validé = false;

            
            Assert.AreEqual(validé, true);
        }

        [TestMethod]

        public void TestConvert2CharTo11Bits()
        {
            char[] he = { 'H', 'E' };
            int[] attendu = { 0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1 };
            int[] result = MyImage.Convert2CharTo11Bits(he);        
            bool validé = true;
            for(int i = 0; i < attendu.Length; i++)
            {
                if (attendu[i] != result[i]) validé = false;
            }

            Assert.AreEqual(validé, true);
        }

        [TestMethod]
        public void TestConvertStringToTabBinaire()
        {
            string entrée = "HELLO WORLD";
            int[][] result = MyImage.ConvertStringToTabBinaire(entrée);

            int[][] attendu = new int[][]
            {
                new int[] { 0,1,1,0,0,0,0,1,0,1,1 },
                new int[] { 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0 },
                new int[] {1,0,0,0,1,0,1,1,1,0,0 },
                new int[] {1,0,1,1,0,1,1,1,0,0,0 },
                new int[] { 1,0,0,1,1,0,1,0,1,0,0 },
                new int[] {0,0,1,1,0,1}
            };
            bool validé = true;
            for(int i = 0; i < 5;i++)
            {
                for (int j = 0; j < 11; j++)
                    if (attendu[i][j] != result[i][j]) validé = false;
            }
            for (int j = 0; j < 6; j++)
                if (result[5][j] != attendu[5][j]) validé = false;

            Assert.AreEqual(validé, true);
        }

        [TestMethod]
        public void TestAlphanumToChar()
        {
            int entrée = 36;
            string attendu = " ";
            string result = MyImage.ConvertAlphanumToChar(entrée);
            bool validé = false;
            if (attendu == result) validé = true;

            Assert.AreEqual(validé, true);

        }
      
    }
}
