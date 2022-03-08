using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PSI___Louis___Meric
{
    class MyImage
    {
        private string myfile;
        private string typeImage;
        private int tailleFichier;
        private int tailleOffset;
        private int hauteurImage;
        private int largeurImage;
        private int nbBitsCouleur;
        private Pixel[,] image;
        private byte[] fichierComplet;

        public string Myfile
        {
            get { return this.myfile; }
        }

        public string TypeImage
        {
            get { return this.typeImage; }
        }

        public int TailleFichier
        {
            get { return this.tailleFichier; }
        }

        public int TailleOffset
        {
            get { return this.tailleOffset; }
        }

        public int HauteurImage
        {
            get { return this.hauteurImage; }
            set { this.hauteurImage = value; }
        }

        public int LargeurImage
        {
            get { return this.largeurImage; }
            set { this.largeurImage = value; }
        }

        public int NbBitsCouleur
        {
            get { return this.nbBitsCouleur; }
        }

        public Pixel[,] Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        public byte[] FichierComplet
        {
            get { return this.fichierComplet; }
            set { this.fichierComplet = value; }
        }

        public MyImage(string typeImage, int tailleFichier, int tailleOffset, int hauteurImage, int largeurImage, int nbBitsCouleur, Pixel[,] image)
        {
            this.typeImage = typeImage;
            this.tailleFichier = tailleFichier;
            this.tailleOffset = tailleOffset;       //constante
            this.hauteurImage = hauteurImage;
            this.largeurImage = largeurImage;
            this.nbBitsCouleur = nbBitsCouleur;     //cst
            this.image = image;
            //this.fichierComplet = fichierComplet;
        }

        public MyImage(string myfile)
        {
            byte[] tab = File.ReadAllBytes(myfile);
            this.typeImage = "Pas BitMap";
            if (tab[0] == (byte)66) if (tab[1] == (byte)77) this.typeImage = "BitMap";    //si les premiers nombres du header sont 66 et 77, alors c'est du .BMP

            if (this.typeImage == "BitMap")
            {

                byte[] tailleFichierEndian = new byte[4];                               //calcul de la taille totale du fichier (image + header)
                for (int i = 2; i < 6; i++) tailleFichierEndian[i - 2] = tab[i];
                this.tailleFichier = Convert_Endian_To_Int(tailleFichierEndian);

                byte[] tailleOffsetEndian = new byte[4];                                 //taille de l'offset
                for (int i = 34; i < 38; i++) tailleOffsetEndian[i - 34] = tab[i];
                this.tailleOffset = Convert_Endian_To_Int(tailleOffsetEndian);

                byte[] largeurImageEndian = new byte[4];                                //taille de la largeur de l'image
                for (int i = 18; i < 22; i++) largeurImageEndian[i - 18] = tab[i];
                this.largeurImage = Convert_Endian_To_Int(largeurImageEndian);

                byte[] hauteurImageEndian = new byte[4];                                    //calcul taille de la hauteur de l'image
                for (int i = 22; i < 26; i++) hauteurImageEndian[i - 22] = tab[i];
                this.hauteurImage = Convert_Endian_To_Int(hauteurImageEndian);

                byte[] nbBitsCouleurEndian = new byte[2];                               //profondeur de codage de la couleur
                for (int i = 28; i < 30; i++) nbBitsCouleurEndian[i - 28] = tab[i];
                this.nbBitsCouleur = Convert_Endian_To_Int(nbBitsCouleurEndian);

                Pixel[,] limage = new Pixel[hauteurImage, largeurImage];            //remplissage de l'attribut du tableau de pixel
                int x = 0;
                int y = 0;
                for (int i = 54; i < tab.Length - 2; i += 3)
                {
                    limage[x, y] = new Pixel(0, 0, 0);
                    limage[x, y].B = tab[i];
                    limage[x, y].G = tab[i + 1];
                    limage[x, y].R = tab[i + 2];
                    if (y < largeurImage - 1) y++;
                    else
                    {
                        y = 0;
                        x++;
                    }
                }
                this.image = limage;
                this.myfile = myfile;
                this.fichierComplet = tab;
            }
        }

        public void From_Image_To_File(string file)
        {
            int coefHauteur = 0;
            int coefLargeur = 0;
            if (this.largeurImage % 4 == 3) coefLargeur = 1;
            if (this.largeurImage % 4 == 2) coefLargeur = 2;
            if (this.largeurImage % 4 == 1) coefLargeur = 3;
            if (this.hauteurImage % 4 == 3) coefHauteur = 1;
            if (this.hauteurImage % 4 == 2) coefHauteur = 2;
            if (this.hauteurImage % 4 == 1) coefHauteur = 3;
            //début recopiage header + header info
            byte[] nouveauFichier = new byte[this.tailleFichier + coefHauteur * (this.largeurImage + coefLargeur) * 3 + coefLargeur * this.hauteurImage * 3];
            nouveauFichier[0] = Convert.ToByte(66);
            nouveauFichier[1] = Convert.ToByte(77);
            byte[] tailleFichierEndian = Convert_Int_To_Endian(this.tailleFichier);
            for (int i = 2; i < 6; i++) nouveauFichier[i] = tailleFichierEndian[i - 2];
            for (int i = 6; i < 14; i++) nouveauFichier[i] = Convert.ToByte(0);
            nouveauFichier[10] = (byte)54;
            for (int i = 11; i < 14; i++) nouveauFichier[i] = (byte)0;
            nouveauFichier[14] = Convert.ToByte(40);
            for (int i = 15; i < 18; i++) nouveauFichier[i] = (byte)0;
            for (int i = 18; i < 22; i++) nouveauFichier[i] = Convert_Int_To_Endian(this.largeurImage)[i - 18];
            for (int i = 22; i < 26; i++) nouveauFichier[i] = Convert_Int_To_Endian(this.hauteurImage)[i - 22];
            nouveauFichier[26] = Convert.ToByte(1);
            nouveauFichier[27] = Convert.ToByte(0);
            for (int i = 28; i < 30; i++) nouveauFichier[i] = Convert_Int_To_Endian(this.nbBitsCouleur)[i - 28];
            for (int i = 30; i < 34; i++) nouveauFichier[i] = Convert.ToByte(0);
            for (int i = 34; i < 38; i++) nouveauFichier[i] = Convert_Int_To_Endian(this.tailleOffset)[i - 34];
            for (int i = 38; i < 54; i++) nouveauFichier[i] = Convert.ToByte(0); //fin recopiage header + header info
            int x = 0;
            int y = 0;
            for (int i = 54; i < nouveauFichier.Length - 2; i += 3)             //copie des octets s'occupant de la couleur et remplissage du tableau de pixel en fct
            {
                if (x != this.hauteurImage)
                {
                    nouveauFichier[i] = this.image[x, y].B;
                    nouveauFichier[i + 1] = this.image[x, y].G;
                    nouveauFichier[i + 2] = this.image[x, y].R;
                    if (y < largeurImage - 1) y++;
                    else
                    {
                        if (this.largeurImage % 4 == 3)
                        {
                            nouveauFichier[i + 3] = 0;
                            i++;
                        }
                        if (this.largeurImage % 4 == 2)
                        {
                            nouveauFichier[i + 3] = 0;
                            nouveauFichier[i + 4] = 0;
                            i += 2;
                        }
                        if (this.largeurImage % 4 == 1)
                        {
                            nouveauFichier[i + 3] = 0;
                            nouveauFichier[i + 4] = 0;
                            nouveauFichier[i + 5] = 0;
                            i += 3;
                        }
                        y = 0;
                        x++;
                    }
                }
                else
                {
                    nouveauFichier[i] = 0;
                    nouveauFichier[i + 1] = 0;
                    nouveauFichier[i + 2] = 0;
                }
            }
            this.fichierComplet = nouveauFichier;
            File.WriteAllBytes(file, nouveauFichier);
        }

        public int Convert_Endian_To_Int(byte[] tab)        //Convertir du base 256 little endian en base 10
        {
            int res = 0;
            for (int i = 0; i < tab.Length; i++)
            {
                if (i == 0) res += tab[i];
                if (i == 1) res += tab[i] * 256;
                if (i == 2) res += tab[i] * 256 * 256;
                if (i == 3) res += tab[i] * 256 * 256 * 256;
            }
            return res;
        }

        public byte[] Convert_Int_To_Endian(int val)        //Convertir du base 10 en base 256 en little endian
        {
            byte[] tab = new byte[4];
            if (val >= (256 * 256 * 256))
            {
                tab[3] = Convert.ToByte(val / (256 * 256 * 256));
                val = val % (256 * 256 * 256);
            }
            if (val >= (256 * 256))
            {
                tab[2] = Convert.ToByte(val / (256 * 256));
                val = val % (256 * 256);
            }
            if (val >= 256)
            {
                tab[1] = Convert.ToByte(val / 256);
                val = val % 256;
            }
            if (val >= 0) tab[0] = Convert.ToByte(val);
            return tab;
        }
        public MyImage Inverse()                        //return l'image avec les coleurs inversés en fct du spectre
        {
            MyImage nouvelleImage = new MyImage(this.Myfile);
            for (int i = 0; i < this.Image.GetLength(0); i++)
            {
                for (int j = 0; j < this.Image.GetLength(1); j++)
                {
                    nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                    nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                    nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                }
            }
            return nouvelleImage;
        }

        public MyImage NuanceDeGris()            //return l'image en nuance de gris
        {
            MyImage nouvelleImage = new MyImage(this.Myfile);
            for (int i = 0; i < this.Image.GetLength(0); i++)
            {
                for (int j = 0; j < this.Image.GetLength(1); j++)
                {
                    nouvelleImage.Image[i, j].R = (byte)((this.Image[i, j].R + this.Image[i, j].G + this.Image[i, j].B) / 3);
                    nouvelleImage.Image[i, j].G = (byte)((this.Image[i, j].R + this.Image[i, j].G + this.Image[i, j].B) / 3);
                    nouvelleImage.Image[i, j].B = (byte)((this.Image[i, j].R + this.Image[i, j].G + this.Image[i, j].B) / 3);
                }
            }
            return nouvelleImage;
        }

        public MyImage NoirEtBlanc()            //return l'image en noir et blanc
        {
            MyImage nouvelleImage = new MyImage(this.Myfile);
            for (int i = 0; i < this.Image.GetLength(0); i++)
            {
                for (int j = 0; j < this.Image.GetLength(1); j++)
                {
                    if ((this.Image[i, j].R + this.Image[i, j].G + this.Image[i, j].B) / 3 < 128)
                    {
                        nouvelleImage.Image[i, j].R = (byte)0;
                        nouvelleImage.Image[i, j].G = (byte)0;
                        nouvelleImage.Image[i, j].B = (byte)0;
                    }
                    else
                    {
                        nouvelleImage.Image[i, j].R = (byte)255;
                        nouvelleImage.Image[i, j].G = (byte)255;
                        nouvelleImage.Image[i, j].B = (byte)255;
                    }
                }
            }
            return nouvelleImage;
        }

        public MyImage MiroirHorizontal()                 //return l'image avec un effet miroir horizontal
        {
            MyImage nouvelleImage = new MyImage(this.myfile);
            for (int i = 0; i < this.image.GetLength(0); i++)
            {
                for (int j = 0; j < this.image.GetLength(1); j++)
                {
                    nouvelleImage.Image[i, j].R = (this.Image[i, this.largeurImage - j - 1].R);
                    nouvelleImage.Image[i, j].G = (this.Image[i, this.largeurImage - j - 1].G);
                    nouvelleImage.Image[i, j].B = (this.Image[i, this.largeurImage - j - 1].B);
                }
            }
            return nouvelleImage;
        }

        public MyImage MiroirVertical()                 //return l'image avec un effet miroir vertical
        {
            MyImage nouvelleImage = new MyImage(this.myfile);
            for (int i = 0; i < this.image.GetLength(0); i++)
            {
                for (int j = 0; j < this.image.GetLength(1); j++)
                {
                    nouvelleImage.Image[i, j].R = (this.Image[this.hauteurImage - i - 1, j].R);
                    nouvelleImage.Image[i, j].G = (this.Image[this.hauteurImage - i - 1, j].G);
                    nouvelleImage.Image[i, j].B = (this.Image[this.hauteurImage - i - 1, j].B);
                }
            }
            return nouvelleImage;
        }

        public MyImage Rotation(int angleDonne)                     //return une image tournée de -angleDonne- vers la droite
        {
            int angleDegre = angleDonne % 360;
            double angleRadian = (double)(angleDegre * Math.PI / 180);
            int newHauteur = (int)Math.Abs((this.hauteurImage * Math.Cos(angleRadian)) + Math.Abs(this.largeurImage * Math.Sin(angleRadian)));
            int newLargeur = (int)(Math.Abs(this.largeurImage * Math.Cos(angleRadian)) + Math.Abs(this.hauteurImage * Math.Cos((Math.PI / 2) - angleRadian)));  //ALEDDDDDDD
            int newTaille = newHauteur * newLargeur * 3 + 54;                                                                               //pour l'instant je cherche à créer les bords mais aled enfait
            Pixel[,] newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);              //enlever la ligne quand la fct sera finie (nan il faut la garder c'est important !)

            for (int i = 0; i < (int)Math.Abs(Math.Cos(angleRadian) * this.hauteurImage - 1); i++)                    //grisage coin haut gauche
            {
                for (int j = 0; j < (int)i * Math.Abs(Math.Tan(angleRadian)); j++)
                {
                    newImage[i + (int)(Math.Abs(Math.Sin(angleRadian) * this.largeurImage)), j].R = (byte)128;
                    newImage[i + (int)Math.Abs(Math.Sin(angleRadian) * this.largeurImage), j].G = (byte)128;
                    newImage[i + (int)Math.Abs(Math.Sin(angleRadian) * this.largeurImage), j].B = (byte)128;
                }
            }

            for (int i = (int)Math.Abs(Math.Sin(angleRadian) * this.hauteurImage); i >= 0; i--)          //grisage coin bas gauche
            {
                for (int j = 0; j < ((int)Math.Abs(Math.Sin(angleRadian) * this.hauteurImage - i) * Math.Abs(Math.Tan(angleRadian))); j++)
                {
                    newImage[i, j].R = (byte)128;
                    newImage[i, j].B = (byte)128;
                    newImage[i, j].G = (byte)128;
                }
            }
            /*for (int i=0; i<newHauteur/2; i++)
            {
                for (int j=0; j<newLargeur; j++)
                {
                    newImage[i, j].R = 255;
                }
            }*/


            /*int x = 0;
            int y = 0;
            //int compteur = 0;
            for (int i = (int)(this.largeurImage * Math.Abs(Math.Sin(angleRadian))); i < newHauteur; i++)
            {
                for (int j = i - (int)(this.largeurImage * Math.Abs(Math.Sin(angleRadian))); j < (int)(this.hauteurImage * Math.Abs(Math.Sin(angleRadian))); j++)
                {
                    newImage[i, j] = this.image[x, y];
                    if (y < this.largeurImage - 1) y++;
                    if (x < this.hauteurImage - 1)
                    {
                        x += (int)(Math.Abs(Math.Sin(angleRadian)));
                        y += (int)(Math.Abs(Math.Cos(angleRadian)));
                    }
                    else
                    {
                        y = 0;
                        x++;
                        //compteur++;
                        //x=compteur;
                    }
                }
            }

            x = this.hauteurImage-1;
            y = 0;
            for (int i = newHauteur-1; i>(int)(this.largeurImage*Math.Abs(Math.Sin(angleRadian))); i--)
            {
                for (int j = (int)(this.hauteurImage * Math.Abs(Math.Sin(angleRadian))); j < newLargeur - (i - (int)(this.largeurImage * Math.Abs(Math.Sin(angleRadian)))); j++)
                {
                    newImage[i, j] = this.image[x, y];
                    if (y < this.largeurImage - 1) y++;
                    if (x < this.hauteurImage - 1)
                    {
                        x += (int)(Math.Abs(Math.Sin(angleRadian)));
                        y += (int)(Math.Abs(Math.Cos(angleRadian)));
                    }
                    else
                    {
                        y = 0;
                        x--;
                        //compteur++;
                        //x=compteur;
                    }
                }
            }*/

            //for (int i=0; i < (int)(this.largeurImage * Math.Sin(angleRadian)); i++)
            //{
            //    for (int j=0; j < (int)(this.largeurImage * Math.Cos(angleRadian))-i; j++)
            //    {
            //        newImage[i, j].R = (byte)128;
            //        newImage[i, j].G = (byte)128;
            //        newImage[i, j].B = (byte)128;
            //    }
            //}

            //for (int i=0; i < (int)(this.hauteurImage*Math.Sin((Math.PI/2)-angleRadian)); i++)
            //{
            //    for (int j = newLargeur - 1; j >= (int)(this.hauteurImage * Math.Cos((Math.PI / 2) - angleRadian) + 1) + i; j--)
            //    {
            //        newImage[i, j].R = (byte)128;
            //        newImage[i, j].G = (byte)128;
            //        newImage[i, j].B = (byte)128;
            //    }
            //}

            //for (int i=newHauteur - 1; i >= (int)(this.hauteurImage * Math.Cos(angleRadian)); i--)
            //{
            //    for (int j = 0; j < i - (int)(this.largeurImage * Math.Cos(angleRadian)); j++)
            //    {
            //        newImage[i, j].R = (byte)128;
            //        newImage[i, j].G = (byte)128;
            //        newImage[i, j].B = (byte)128;
            //    }
            //}

            //for (int i = (int)(this.largeurImage * Math.Sin(angleRadian))+1; i < newHauteur; i++)
            //{
            //    for (int j = newLargeur - 1; j >= - i + newHauteur + (int)(this.largeurImage * Math.Sin(angleRadian)); j--)
            //    {
            //        newImage[i, j].R = (byte)128;
            //        newImage[i, j].G = (byte)128;
            //        newImage[i, j].B = (byte)128;
            //    }
            //}

            MyImage nouvelleImage = new MyImage("BitMap", newTaille, this.TailleOffset, newHauteur, newLargeur, this.NbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage Rotation90Droite()
        {
            int newHauteur = this.largeurImage;
            int newLargeur = this.hauteurImage;
            Pixel[,] newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);
            for (int i = 0; i < newHauteur; i++)
            {
                for (int j = 0; j < newLargeur; j++)
                {
                    newImage[i, j].R = this.image[j, newHauteur - i - 1].R;
                    newImage[i, j].G = this.image[j, newHauteur - i - 1].G;
                    newImage[i, j].B = this.image[j, newHauteur - i - 1].B;
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, newHauteur, newLargeur, this.NbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage Rotation90Gauche()
        {
            int newHauteur = this.largeurImage;
            int newLargeur = this.hauteurImage;
            Pixel[,] newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);
            for (int i = 0; i < newHauteur; i++)
            {
                for (int j = 0; j < newLargeur; j++)
                {
                    newImage[i, j].R = this.image[newLargeur - j - 1, i].R;
                    newImage[i, j].G = this.image[newLargeur - j - 1, i].G;
                    newImage[i, j].B = this.image[newLargeur - j - 1, i].B;
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, newHauteur, newLargeur, this.NbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage Reduire(int coefHauteur, int coefLargeur)                    //return l'image réduite -coefHauteur * coefLargeur- fois
        {
            int newTaille = (this.hauteurImage / coefHauteur) * (this.LargeurImage / coefLargeur) * 3 + 54;
            int newHauteur = this.hauteurImage / coefHauteur;
            int newLargeur = this.largeurImage / coefLargeur;
            int newTailleOffset = newHauteur * newLargeur * 3;
            Pixel[,] newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);

            int x = 0;
            int y = 0;
            for (int i = 0; i < hauteurImage - coefHauteur + 1; i += coefHauteur)
            {
                for (int j = 0; j < largeurImage - coefLargeur + 1; j += coefLargeur)
                {
                    newImage[x, y] = new Pixel(image[i, j].R, image[i, j].G, image[i, j].B);
                    if (y < newLargeur - 1) y++;
                    else
                    {
                        y = 0;
                        x++;
                    }
                }
            }
            MyImage imageReduite = new MyImage("BitMap", newTaille, newTailleOffset, newHauteur, newLargeur, this.NbBitsCouleur, newImage);
            return imageReduite;
        }

        public MyImage Agrandir(int coefHauteur, int coefLargeur)                       //return l'image aggrandie -coefHauteur * coefLargeur- fois
        {
            int newTaille = this.hauteurImage * coefHauteur * this.LargeurImage * coefLargeur * 3 + 54;
            int newHauteur = this.hauteurImage * coefHauteur;
            int newLargeur = this.largeurImage * coefLargeur;
            int newTailleOffset = newHauteur * newLargeur * 3;
            Pixel[,] newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);

            int x = 0;
            int y = 0;
            for (int i = 0; i < newHauteur; i += coefHauteur)
            {
                for (int j = 0; j < newLargeur; j += coefLargeur)
                {
                    newImage[i, j] = new Pixel(image[x, y].R, image[x, y].G, image[x, y].B);
                    for (int n = 0; n < coefHauteur; n++)
                    {
                        for (int k = 0; k < coefLargeur; k++)
                        {
                            newImage[i + n, j + k] = new Pixel(image[x, y].R, image[x, y].G, image[x, y].B);
                        }
                    }
                    if (y < largeurImage - 1) y++;
                    else
                    {
                        y = 0;
                        x++;
                    }
                }
            }
            MyImage imageAgrandie = new MyImage("BitMap", newTaille, newTailleOffset, newHauteur, newLargeur, this.NbBitsCouleur, newImage);
            return imageAgrandie;
        }

        public MyImage Flou()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 2, 2, 2 }, { 2, 2, 2 }, { 2, 2, 2 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    newImage[i, j].R = (byte)(calculR / 18);
                    newImage[i, j].G = (byte)(calculG / 18);
                    newImage[i, j].B = (byte)(calculB / 18);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                newImage[i, 0].R = (byte)(calculR / 18);
                newImage[i, 0].G = (byte)(calculG / 18);
                newImage[i, 0].B = (byte)(calculB / 18);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                newImage[i, this.largeurImage - 1].R = (byte)(calculR / 18);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG / 18);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB / 18);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                newImage[0, j].R = (byte)(calculR / 18);
                newImage[0, j].G = (byte)(calculG / 18);
                newImage[0, j].B = (byte)(calculB / 18);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR / 18);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG / 18);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB / 18);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            newImage[0, 0].R = (byte)(calculR / 18);
            newImage[0, 0].G = (byte)(calculG / 18);
            newImage[0, 0].B = (byte)(calculB / 18);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            newImage[0, this.largeurImage - 1].R = (byte)(calculR / 18);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG / 18);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB / 18);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR / 18);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG / 18);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB / 18);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR / 18);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG / 18);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB / 18);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage Psychedelique()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage DetectionContours()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.LargeurImage];
            int[,] matConvo = new int[,] { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };

            for (int i = 0; i < newImage.GetLength(0); i++)
                for (int j = 0; j < newImage.GetLength(1); j++)
                    newImage[i, j] = new Pixel(0, 0, 0);                        //à optimiser qd fct finie

            for (int i = 1; i < newImage.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < newImage.GetLength(1) - 1; j++)
                {
                    int sommeR = 0;
                    int sommeB = 0;
                    int sommeG = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            sommeR += this.image[i - 1 + k, j - 1 + l].R * matConvo[k, l];
                            sommeB += this.image[i - 1 + k, j - 1 + l].B * matConvo[k, l];
                            sommeG += this.image[i - 1 + k, j - 1 + l].G * matConvo[k, l];
                        }
                    }
                    if ((sommeR + sommeB + sommeG) / 3 > 255)
                    {
                        newImage[i, j].R = 255;
                        newImage[i, j].B = 255;
                        newImage[i, j].G = 255;
                    }
                    else if ((sommeR + sommeB + sommeG) / 3 < 0)
                    {
                        newImage[i, j].R = 0;
                        newImage[i, j].B = 0;
                        newImage[i, j].G = 0;
                    }
                    else
                    {
                        newImage[i, j].R = (byte)((sommeR + sommeB + sommeG) / 3);
                        newImage[i, j].B = (byte)((sommeR + sommeB + sommeG) / 3);
                        newImage[i, j].G = (byte)((sommeR + sommeB + sommeG) / 3);
                    }
                }
            }
            MyImage areturn = new MyImage("BitMap", this.TailleFichier, this.TailleOffset, this.HauteurImage, this.LargeurImage, this.NbBitsCouleur, newImage);
            return areturn;
        }

        public MyImage AugmentationContraste()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    if (calculR > 255) calculR = 255;
                    if (calculG > 255) calculG = 255;
                    if (calculB > 255) calculB = 255;
                    if (calculR < 0) calculR = 0;
                    if (calculG < 0) calculG = 0;
                    if (calculB < 0) calculB = 0;
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage RenforcementBordsVertical()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 0, -4, 0 }, { 0, 4, 0 }, { 0, 0, 0 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    if (calculR > 255) calculR = 255;
                    if (calculG > 255) calculG = 255;
                    if (calculB > 255) calculB = 255;
                    if (calculR < 0) calculR = 0;
                    if (calculG < 0) calculG = 0;
                    if (calculB < 0) calculB = 0;
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage RenforcementBordsHorizontal()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 0, 0, 0 }, { -4, 4, 0 }, { 0, 0, 0 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    if (calculR > 255) calculR = 255;
                    if (calculG > 255) calculG = 255;
                    if (calculB > 255) calculB = 255;
                    if (calculR < 0) calculR = 0;
                    if (calculG < 0) calculG = 0;
                    if (calculB < 0) calculB = 0;
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage Repoussage()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 0, 1, 2 }, { -1, 1, 1 }, { -2, -1, 0 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    if (calculR > 255) calculR = 255;
                    if (calculG > 255) calculG = 255;
                    if (calculB > 255) calculB = 255;
                    if (calculR < 0) calculR = 0;
                    if (calculG < 0) calculG = 0;
                    if (calculB < 0) calculB = 0;
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage FiltreSobel()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 0 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    if (calculR > 255) calculR = 255;
                    if (calculG > 255) calculG = 255;
                    if (calculB > 255) calculB = 255;
                    if (calculR < 0) calculR = 0;
                    if (calculG < 0) calculG = 0;
                    if (calculB < 0) calculB = 0;
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage ConvolutionAleatoire()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int calculR = 0;
            int calculG = 0;
            int calculB = 0;
            int[,] matConvolution = { { 1, 0, 1 }, { 0, -3, 0 }, { 1, 0, 1 } };
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //coeur de l'image
            {
                for (int j = 1; j < this.largeurImage - 1; j++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            calculR += this.image[i + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                    if (calculR > 255) calculR = 255;
                    if (calculG > 255) calculG = 255;
                    if (calculB > 255) calculB = 255;
                    if (calculR < 0) calculR = 0;
                    if (calculG < 0) calculG = 0;
                    if (calculB < 0) calculB = 0;
                    newImage[i, j].R = (byte)(calculR);
                    newImage[i, j].G = (byte)(calculG);
                    newImage[i, j].B = (byte)(calculB);
                    calculR = 0;
                    calculG = 0;
                    calculB = 0;
                }
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord gauche
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[i + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[i + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[i + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, 0].R = (byte)(calculR);
                newImage[i, 0].G = (byte)(calculG);
                newImage[i, 0].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int i = 1; i < this.hauteurImage - 1; i++)                           //bord droit
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (y != 1)
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[i + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[i + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[i + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[i, this.largeurImage - 1].R = (byte)(calculR);
                newImage[i, this.largeurImage - 1].G = (byte)(calculG);
                newImage[i, this.largeurImage - 1].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord bas
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        calculR += this.image[Math.Abs(0 + x), j + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), j + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), j + y].B * matConvolution[x + 1, y + 1];
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[0, j].R = (byte)(calculR);
                newImage[0, j].G = (byte)(calculG);
                newImage[0, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }
            //bord haut
            for (int j = 1; j < this.largeurImage - 1; j++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 1)
                        {
                            calculR += this.image[this.hauteurImage - 1 + x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 + x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 + x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                        else
                        {
                            calculR += this.image[this.hauteurImage - 1 - x, j + y].R * matConvolution[x + 1, y + 1];
                            calculG += this.image[this.hauteurImage - 1 - x, j + y].G * matConvolution[x + 1, y + 1];
                            calculB += this.image[this.hauteurImage - 1 - x, j + y].B * matConvolution[x + 1, y + 1];
                        }
                    }
                }
                if (calculR > 255) calculR = 255;
                if (calculG > 255) calculG = 255;
                if (calculB > 255) calculB = 255;
                if (calculR < 0) calculR = 0;
                if (calculG < 0) calculG = 0;
                if (calculB < 0) calculB = 0;
                newImage[this.hauteurImage - 1, j].R = (byte)(calculR);
                newImage[this.hauteurImage - 1, j].G = (byte)(calculG);
                newImage[this.hauteurImage - 1, j].B = (byte)(calculB);
                calculR = 0;
                calculG = 0;
                calculB = 0;
            }

            for (int x = -1; x <= 1; x++)                       //coin bas gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    calculR += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                    calculG += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                    calculB += this.image[Math.Abs(0 + x), Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, 0].R = (byte)(calculR);
            newImage[0, 0].G = (byte)(calculG);
            newImage[0, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin bas droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y != 1)
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[Math.Abs(0 + x), this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[0, this.largeurImage - 1].R = (byte)(calculR);
            newImage[0, this.largeurImage - 1].G = (byte)(calculG);
            newImage[0, this.largeurImage - 1].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut gauche
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                    else
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, Math.Abs(0 + y)].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, 0].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, 0].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, 0].B = (byte)(calculB);
            calculR = 0;
            calculG = 0;
            calculB = 0;

            for (int x = -1; x <= 1; x++)                       //coin haut droite
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y != 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 + y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x != 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 + x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                    else if (x == 1 && y == 1)
                    {
                        calculR += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].R * matConvolution[x + 1, y + 1];
                        calculG += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].G * matConvolution[x + 1, y + 1];
                        calculB += this.image[this.hauteurImage - 1 - x, this.largeurImage - 1 - y].B * matConvolution[x + 1, y + 1];
                    }
                }
            }
            if (calculR > 255) calculR = 255;
            if (calculG > 255) calculG = 255;
            if (calculB > 255) calculB = 255;
            if (calculR < 0) calculR = 0;
            if (calculG < 0) calculG = 0;
            if (calculB < 0) calculB = 0;
            newImage[this.hauteurImage - 1, this.largeurImage - 1].R = (byte)(calculR);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].G = (byte)(calculG);
            newImage[this.hauteurImage - 1, this.largeurImage - 1].B = (byte)(calculB);

            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public MyImage Histogramme()
        {
            Pixel[,] newImage = new Pixel[100, 512];
            for (int i = 0; i < 100; i++) for (int j = 0; j < 512; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int taille = this.hauteurImage * this.largeurImage;
            int[] tabCompteurCouleurs = new int[8 * 8 * 8];
            int zoneR;
            int zoneG;
            int zoneB;
            for (int i=0; i<this.hauteurImage; i++)
            {
                for (int j=0; j<largeurImage; j++)
                {
                    if (this.image[i, j].R < 32) zoneR = 0;
                    else if (this.image[i, j].R < 64) zoneR = 1;
                    else if (this.image[i, j].R < 96) zoneR = 2;
                    else if (this.image[i, j].R < 128) zoneR = 3;
                    else if (this.image[i, j].R < 160) zoneR = 4;
                    else if (this.image[i, j].R < 192) zoneR = 5;
                    else if (this.image[i, j].R < 224) zoneR = 6;
                    else zoneR = 7;

                    if (this.image[i, j].G < 32) zoneG = 0;
                    else if (this.image[i, j].G < 64) zoneG = 1;
                    else if (this.image[i, j].G < 96) zoneG = 2;
                    else if (this.image[i, j].G < 128) zoneG = 3;
                    else if (this.image[i, j].G < 160) zoneG = 4;
                    else if (this.image[i, j].G < 192) zoneG = 5;
                    else if (this.image[i, j].G < 224) zoneG = 6;
                    else zoneG = 7;

                    if (this.image[i, j].B < 32) zoneB = 0;
                    else if (this.image[i, j].B < 64) zoneB = 1;
                    else if (this.image[i, j].B < 96) zoneB = 2;
                    else if (this.image[i, j].B < 128) zoneB = 3;
                    else if (this.image[i, j].B < 160) zoneB = 4;
                    else if (this.image[i, j].B < 192) zoneB = 5;
                    else if (this.image[i, j].B < 224) zoneB = 6;
                    else zoneB = 7;

                    tabCompteurCouleurs[zoneR * 64 + zoneG * 8 + zoneB]++;
                }
            }
            int max = tabCompteurCouleurs[0];
            for (int i = 0; i < tabCompteurCouleurs.Length; i++) if (max < tabCompteurCouleurs[i]) max = tabCompteurCouleurs[i];
            for (int j = 0; j < 512; j++)
            {
                for (int x=0; x<(tabCompteurCouleurs[j]*100/max); x++)
                {
                    newImage[x, j] = new Pixel(255, 0, 0);
                }
            }
            int tailleFichier = 100 * 512 * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, 100, 512, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }

        public int[] Convert_Byte_To_Hexadecimal(byte valeur)
        {
            int[] tabHexadecimal = new int[8];
            for (int i=0; i<8; i++)
            {
                tabHexadecimal[i] = (byte)(valeur / Math.Pow(2, 7 - i));
                valeur = (byte)(valeur % Math.Pow(2, 7 - i));
            }
            return tabHexadecimal;
        }

        public byte Convert_Hexadecimal_To_Byte(int[] tabHexadecimal)
        {
            byte valeur = 0;
            for (int i=0; i<8; i++)
            {
                valeur += (byte)(tabHexadecimal[i] * Math.Pow(2, 7 - i));
            }
            return valeur;
        }

        public MyImage CacherImage(string nomImage2)
        {
            MyImage image2 = new MyImage(nomImage2);
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            int minHauteur;
            int minLargeur;
            if (this.hauteurImage < image2.hauteurImage) minHauteur = this.hauteurImage;
            else minHauteur = image2.hauteurImage;
            if (this.largeurImage < image2.largeurImage) minLargeur = this.largeurImage;
            else minLargeur = image2.largeurImage;
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = this.image[i, j];
            for (int i = 0; i < minHauteur; i++)
            {
                for (int j = 0; j < minLargeur; j++)
                {
                    int[] hexaImage1R = Convert_Byte_To_Hexadecimal(this.image[i, j].R);
                    int[] hexaImage2R = Convert_Byte_To_Hexadecimal(image2.image[i, j].R);
                    int[] newHexaR = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaR[k] = hexaImage1R[k];
                        else newHexaR[k] = hexaImage2R[k];
                    }
                    newImage[i, j].R = Convert_Hexadecimal_To_Byte(newHexaR);
                    int[] hexaImage1G = Convert_Byte_To_Hexadecimal(this.image[i, j].G);
                    int[] hexaImage2G = Convert_Byte_To_Hexadecimal(image2.image[i, j].G);
                    int[] newHexaG = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaG[k] = hexaImage1G[k];
                        else newHexaG[k] = hexaImage2G[k];
                    }
                    newImage[i, j].G = Convert_Hexadecimal_To_Byte(newHexaG);
                    int[] hexaImage1B = Convert_Byte_To_Hexadecimal(this.image[i, j].B);
                    int[] hexaImage2B = Convert_Byte_To_Hexadecimal(image2.image[i, j].B);
                    int[] newHexaB = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaB[k] = hexaImage1B[k];
                        else newHexaB[k] = hexaImage2B[k];
                    }
                    newImage[i, j].B = Convert_Hexadecimal_To_Byte(newHexaB);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }
    }
}