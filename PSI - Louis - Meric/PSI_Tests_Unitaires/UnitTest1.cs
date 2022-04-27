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
            char[] he = { 'h', 'e' };
            int[] attendu = { 0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1 };
            int[] result = MyImage.Convert2CharTo11Bits(he);        //result est null pcq convert2charTo11Bits appelle une autre méthode, jsp comment faire
            bool validé = true;
            for(int i = 0; i < attendu.Length; i++)
            {
                if (attendu[i] != result[i]) validé = false;
            }

            Assert.AreEqual(validé, true);
        }
    }
}
