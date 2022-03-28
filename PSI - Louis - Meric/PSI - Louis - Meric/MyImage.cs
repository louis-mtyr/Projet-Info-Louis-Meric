﻿using System;
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
            int coefLargeur = 0;
            if (this.largeurImage*3 % 4 == 3) coefLargeur = 1;
            if (this.largeurImage*3 % 4 == 2) coefLargeur = 2;
            if (this.largeurImage*3 % 4 == 1) coefLargeur = 3;
            //début recopiage header + header info
            byte[] nouveauFichier = new byte[this.tailleFichier + coefLargeur * this.hauteurImage * 3];
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
                        if (this.largeurImage*3 % 4 == 3)
                        {
                            nouveauFichier[i + 3] = 0;
                            i++;
                        }
                        if (this.largeurImage*3 % 4 == 2)
                        {
                            nouveauFichier[i + 3] = 0;
                            nouveauFichier[i + 4] = 0;
                            i += 2;
                        }
                        if (this.largeurImage*3 % 4 == 1)
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

        public MyImage CouleurAléatoire()
        {
            MyImage nouvelleImage = new MyImage(this.Myfile);
            Random aleatoire = new Random();
            int tirage = aleatoire.Next(1, 48);
            for (int i = 0; i < this.Image.GetLength(0); i++)
            {
                for (int j = 0; j < this.Image.GetLength(1); j++)
                {
                    switch (tirage)
                    {
                        case 1:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 2:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 3:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 4:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 5:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 6:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 7:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 8:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 9:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 10:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 11:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 12:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 13:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 14:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 15:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 16:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 17:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 18:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 19:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 20:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 21:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 22:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].B);
                            break;
                        case 23:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].R);
                            break;
                        case 24:
                            nouvelleImage.Image[i, j].R = (byte)(255 - this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(255 - this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(255 - this.Image[i, j].G);
                            break;
                        case 25:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 26:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 27:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 28:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 29:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 30:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 31:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 32:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 33:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 34:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 35:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 36:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 37:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 38:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 39:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 40:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 41:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 42:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 43:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 44:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 45:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                        case 46:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 47:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                    }
                }
            }
            return nouvelleImage;
        }               //return l'image avec un différent jeu de couleur obtenu de manière aléatoire

        public MyImage Saturage()
        {
            MyImage nouvelleImage = new MyImage(this.Myfile);
            Random aleatoire = new Random();
            int tirage;
            for (int i = 0; i < this.Image.GetLength(0); i++)
            {
                for (int j = 0; j < this.Image.GetLength(1); j++)
                {
                    tirage = aleatoire.Next(1, 4);
                    switch (tirage)
                    {
                        case 1:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].R);
                            break;
                        case 2:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].G);
                            break;
                        case 3:
                            nouvelleImage.Image[i, j].R = (byte)(this.Image[i, j].B);
                            break;
                    }
                    tirage = aleatoire.Next(1, 4);
                    switch (tirage)
                    {
                        case 1:
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].R);
                            break;
                        case 2:
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].G);
                            break;
                        case 3:
                            nouvelleImage.Image[i, j].G = (byte)(this.Image[i, j].B);
                            break;
                    }
                    tirage = aleatoire.Next(1, 4);
                    switch (tirage)
                    {
                        case 1:
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].R);
                            break;
                        case 2:
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].G);
                            break;
                        case 3:
                            nouvelleImage.Image[i, j].B = (byte)(this.Image[i, j].B);
                            break;
                    }
                }
            }
            return nouvelleImage;
        }                   //return l'image avec un effet saturé

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
            int coef = angleDegre / 90;
            double angleRadian;
            int newHauteur;
            int newLargeur;
            int newTaille;
            Pixel[,] newImage;
            bool pixelColor;
            int sommeR;
            int sommeG;
            int sommeB;
            MyImage imageATourner = this;

            for (int x=0; x<coef; x++)
            {
                imageATourner = imageATourner.Rotation90Droite();
            }

            angleRadian = (double)((angleDegre-coef*90) * Math.PI / 180);
            newHauteur = (int)Math.Abs((imageATourner.hauteurImage * Math.Cos(angleRadian)) + Math.Abs(imageATourner.largeurImage * Math.Sin(angleRadian)))+1;
            newLargeur = (int)(Math.Abs(imageATourner.largeurImage * Math.Cos(angleRadian)) + Math.Abs(imageATourner.hauteurImage * Math.Cos((Math.PI / 2) - angleRadian)))+1;  
            newTaille = newHauteur * newLargeur * 3 + 54;                                                                               
            newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);              

            for (int i=0; i < imageATourner.hauteurImage; i++)
            {
                for (int j= 0; j< imageATourner.largeurImage; j++)
                {
                    newImage[(int)(i * Math.Cos(angleRadian) - j * Math.Sin(angleRadian) + imageATourner.largeurImage * Math.Abs(Math.Sin(angleRadian))), (int)Math.Abs(i * Math.Sin(angleRadian) + j * Math.Cos(angleRadian))] = imageATourner.image[i, j];
                }
            }

            pixelColor = true;
            sommeR = 0;
            sommeG = 0;
            sommeB = 0;
            for (int i=1; i<newHauteur-1; i++)
            {
                for (int j=1; j<newLargeur-1; j++)
                {
                    if (newImage[i, j].R == 0 && newImage[i, j].G == 0 && newImage[i, j].B == 0)
                    {
                        sommeR = 0;
                        sommeG = 0;
                        sommeB = 0;
                        pixelColor = true;
                        for (int n = -1; n <= 1; n++)
                        {
                            for (int m = -1; m <= 1; m++)
                            {
                                if (n != 0 || m != 0)
                                {
                                    sommeR += newImage[i + n, j + m].R;
                                    sommeG += newImage[i + n, j + m].G;
                                    sommeB += newImage[i + n, j + m].B;
                                }
                            }
                        }
                            newImage[i, j] = new Pixel((byte)(sommeR / 8), (byte)(sommeG / 8), (byte)(sommeB / 8));
                    }
                }
            }
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
        }                   //return l'image tournée de 90° vers la droite

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
        }                       //return l'image tournée de 90° vers la gauche

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
        }                                      //return l'image avec un effet flou

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
        }                                     //return l'image avec un effet psychédélique

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
        }                               //return la détection des contours de l'image

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
        }                                    //return l'image avec une augmentation de ses contrastes

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
        }                       //return l'image avec un renforcement des bords sur la verticale

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
        }                           //return l'image avec un renforcement des bords sur l'horizontale

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
        }                               //return l'image avec un effet de repoussage

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
        }                                           //return l'image avec un filtre de Sobel appliqué

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
        }                           //return l'image avec un genre d'effet flou obtenu en jouant avec des matrices de convolution

        public MyImage Histogramme()
        {
            Pixel[,] newImage = new Pixel[300, 768];
            for (int i = 0; i < 300; i++) for (int j = 0; j < 768; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int[] tabCouleurR = new int[256];
            int[] tabCouleurG = new int[256];
            int[] tabCouleurB = new int[256];
            for (int i=0; i<this.hauteurImage; i++)
            {
                for (int j=0; j<this.largeurImage; j++)
                {
                    tabCouleurR[this.image[i, j].R]++;
                    tabCouleurG[this.image[i, j].G]++;
                    tabCouleurB[this.image[i, j].B]++;
                }
            }
            int maxR = tabCouleurR[0];
            int maxG = tabCouleurG[0];
            int maxB = tabCouleurB[0];
            for (int i = 0; i < 256; i++)
            {
                if (maxR < tabCouleurR[i]) maxR = tabCouleurR[i];
                if (maxG < tabCouleurG[i]) maxG = tabCouleurG[i];
                if (maxB < tabCouleurB[i]) maxB = tabCouleurB[i];
            }
            for (int j = 0; j < 768; j+=3)
            {
                for (int x = 0; x < (tabCouleurR[j/3] * 300 / maxR); x++)
                {
                    newImage[x, j].R = 255;
                    newImage[x, j+1].R = 255;
                    newImage[x, j+2].R = 255;
                }
                for (int x = 0; x < (tabCouleurG[j/3] * 300 / maxG); x++)
                {
                    newImage[x, j].G = 255;
                    newImage[x, j + 1].G = 255;
                    newImage[x, j + 2].G = 255;
                }
                for (int x = 0; x < (tabCouleurB[j/3] * 300 / maxB); x++)
                {
                    newImage[x, j].B = 255;
                    newImage[x, j + 1].B = 255;
                    newImage[x, j + 2].B = 255;
                }
            }
            
            int tailleFichier = 300 * 768 * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, 300, 768, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                       //return l'histogramme de l'image

        public MyImage DegradeMulticolore()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0,0,0);
            for (int i=0; i<this.hauteurImage; i++)
            {
                for (int j=0; j<this.largeurImage; j++)
                {
                    newImage[i, j].R = (byte)Math.Abs(this.image[i, j].R - i + j);
                    newImage[i, j].G = (byte)Math.Abs(this.image[i, j].G + 2*i - 2*j);
                    newImage[i, j].B = (byte)Math.Abs(this.image[i, j].B - i + j);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                       //return l'image avec un filtre de dégradé multicolore

        public MyImage SymetrieHorizontale()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int k = this.hauteurImage/2;
            for (int i = 0; i < this.hauteurImage/2; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[i, j];
                }
            }
            for (int i = this.hauteurImage/2; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[k, j];
                }
                k--;
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                   //return l'image avec une symétrie sur un axe horizontal

        public MyImage SymetrieVerticale()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int k = this.largeurImage / 2;
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage/2; j++)
                {
                    newImage[i, j] = this.image[i, j];
                }
            }
            for (int i = 0; i < this.hauteurImage; i++)
            {
                k = this.largeurImage / 2;
                for (int j = this.largeurImage/2; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[i, k];
                    k--;
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                   //return l'image avec une symétrie sur un axe vertical

        public MyImage SymetrieCentrale()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int k = this.largeurImage / 2;
            int n = this.hauteurImage / 2;
            for (int i = 0; i < this.hauteurImage / 2; i++)
            {
                for (int j = 0; j < this.largeurImage / 2; j++)
                {
                    newImage[i, j] = this.image[i, j];
                }
            }
            for (int i = 0; i < this.hauteurImage / 2; i++)
            {
                k = this.largeurImage / 2;
                for (int j = this.largeurImage / 2; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[i, k];
                    k--;
                }
            }
            for (int i = this.hauteurImage / 2; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage / 2; j++)
                {
                    newImage[i, j] = this.image[n, j];
                }
                n--;
            }
            k = this.largeurImage / 2;
            n = this.hauteurImage / 2;
            for (int i = this.hauteurImage / 2; i < this.hauteurImage; i++)
            {
                k = this.largeurImage / 2;
                for (int j = this.largeurImage / 2; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[n, k];
                    k--;
                }
                n--;
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                       //return l'image avec une symétrie sur un axe vertical et horizontal

        public int[] Convert_Byte_To_Hexadecimal(byte valeur)
        {
            int[] tabHexadecimal = new int[8];
            for (int i=0; i<8; i++)
            {
                tabHexadecimal[i] = (byte)(valeur / Math.Pow(2, 7 - i));
                valeur = (byte)(valeur % Math.Pow(2, 7 - i));
            }
            return tabHexadecimal;
        }           //return le byte entré en un tableau de int correspond aux valeurs binaires de la valeur en hexadécimal

        public byte Convert_Hexadecimal_To_Byte(int[] tabHexadecimal)
        {
            byte valeur = 0;
            for (int i=0; i<8; i++)
            {
                valeur += (byte)(tabHexadecimal[i] * Math.Pow(2, 7 - i));
            }
            return valeur;
        }           //return le tableau binaire d'un nombre en hexadécimal transformé en byte

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
                        else newHexaR[k] = hexaImage2R[k-4];
                    }
                    newImage[i, j].R = Convert_Hexadecimal_To_Byte(newHexaR);

                    int[] hexaImage1G = Convert_Byte_To_Hexadecimal(this.image[i, j].G);
                    int[] hexaImage2G = Convert_Byte_To_Hexadecimal(image2.image[i, j].G);
                    int[] newHexaG = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaG[k] = hexaImage1G[k];
                        else newHexaG[k] = hexaImage2G[k-4];
                    }
                    newImage[i, j].G = Convert_Hexadecimal_To_Byte(newHexaG);

                    int[] hexaImage1B = Convert_Byte_To_Hexadecimal(this.image[i, j].B);
                    int[] hexaImage2B = Convert_Byte_To_Hexadecimal(image2.image[i, j].B);
                    int[] newHexaB = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaB[k] = hexaImage1B[k];
                        else newHexaB[k] = hexaImage2B[k-4];
                    }
                    newImage[i, j].B = Convert_Hexadecimal_To_Byte(newHexaB);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                   //return une image avec une autre cachée dedans

        public MyImage RetrouverImage()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0,0,0);
            for (int i=0; i<this.hauteurImage; i++)
            {
                for (int j=0; j<this.largeurImage; j++)
                {
                    int[] hexaImageR = Convert_Byte_To_Hexadecimal(this.image[i, j].R);
                    int[] newHexaR = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaR[k] = hexaImageR[k+4];
                        else newHexaR[k] = 0;
                    }
                    newImage[i, j].R = Convert_Hexadecimal_To_Byte(newHexaR);

                    int[] hexaImageG = Convert_Byte_To_Hexadecimal(this.image[i, j].G);
                    int[] newHexaG = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaG[k] = hexaImageG[k + 4];
                        else newHexaG[k] = 0;
                    }
                    newImage[i, j].G = Convert_Hexadecimal_To_Byte(newHexaG);

                    int[] hexaImageB = Convert_Byte_To_Hexadecimal(this.image[i, j].B);
                    int[] newHexaB = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaB[k] = hexaImageB[k + 4];
                        else newHexaB[k] = 0;
                    }
                    newImage[i, j].B = Convert_Hexadecimal_To_Byte(newHexaB);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                                   //return l'image cachée derrière celle fabriquée

        public MyImage FractaleMandelbrot(int hauteur, int largeur)
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int n = 0; n < hauteur; n++) for (int m = 0; m < largeur; m++) newImage[n, m] = new Pixel(0, 0, 0);
            double xMin = -1.25;
            double xMax = 1.25;
            double yMin = -2;
            double yMax = 0.5;
            //int zoom = 100;
            int iteration_max = 50;

            double cx;
            double cy;
            double xn;
            double yn;
            int i = 0;
            double tmp_x;
            double tmp_y;

            for (int x=0; x<hauteur; x++)
            {
                for (int y=0; y<largeur; y++)
                {
                    cy = (x * (xMax - xMin) / hauteur + xMin);
                    cx = (y * (yMax - yMin) / largeur + yMin);
                    xn = 0;
                    yn = 0;
                    i = 0;

                    while ((xn * xn + yn * yn) < 4 && i < iteration_max)
                    {
                        tmp_x = xn;
                        tmp_y = yn;
                        xn = tmp_x * tmp_x - tmp_y * tmp_y + cx;
                        yn = 2 * tmp_x * tmp_y + cy;
                        i++;
                    }
                    if (i != iteration_max) newImage[x, y] = new Pixel((byte)((1*i)%256), (byte)((10*i)%256), (byte)((3*i)%256));
                }
            }
            int tailleFichier = hauteur * largeur * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, hauteur, largeur, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }
    }
}