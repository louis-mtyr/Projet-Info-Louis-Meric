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
        } //correspond au nom de l'image dans le debug

        public string TypeImage
        {
            get { return this.typeImage; }
        } //vaut "BitMap" si c'en est un, n'est pas défini si ce n'en est pas un

        public int TailleFichier
        {
            get { return this.tailleFichier; }
        } //correspond à la taille totale du fichier complet de l'image

        public int TailleOffset
        {
            get { return this.tailleOffset; }
        } //correspond à la taille offset (taille totale - taille header (=54)) de l'image

        public int HauteurImage
        {
            get { return this.hauteurImage; }
            set { this.hauteurImage = value; }
        } //correspond à la hauteur de l'image en nombre de pixels

        public int LargeurImage
        {
            get { return this.largeurImage; }
            set { this.largeurImage = value; }
        } //correspond à la largeur de l'image en nombre de pixels

        public int NbBitsCouleur
        {
            get { return this.nbBitsCouleur; }
        } //vaut 24 par défaut pour les images que nous traitons

        public Pixel[,] Image
        {
            get { return this.image; }
            set { this.image = value; }
        } //correspond à notre matrice de pixels (autrement dit notre image)

        public byte[] FichierComplet
        {
            get { return this.fichierComplet; }
            set { this.fichierComplet = value; }
        } //correspond au tableau de byte que nous remplissons pour créer notre image sous forme de fichier lisible par l'ordinateur

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
        } //permet de créer un MyImage en utilisant directement les bons champs

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
        } //permet de créer un MyImage à partir d'une image au format .bmp 24 bits déjà existante

        /// <summary>
        /// Recupère le this.FichierComplet du MyImage utilisé pour l'écrire dans un fichier au nom de "file" sur l'ordinateur
        /// </summary>
        /// <param name="file">Le nom donné au nouveau fichier</param>
        public void From_Image_To_File(string file)
        {
            int coefLargeur = 0;
            if (this.largeurImage * 3 % 4 == 3) coefLargeur = 1;
            if (this.largeurImage * 3 % 4 == 2) coefLargeur = 2;
            if (this.largeurImage * 3 % 4 == 1) coefLargeur = 3;
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
                        if (this.largeurImage * 3 % 4 == 3)
                        {
                            nouveauFichier[i + 3] = 0;
                            i++;
                        }
                        if (this.largeurImage * 3 % 4 == 2)
                        {
                            nouveauFichier[i + 3] = 0;
                            nouveauFichier[i + 4] = 0;
                            i += 2;
                        }
                        if (this.largeurImage * 3 % 4 == 1)
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
        } //permet d'écrire le tableau de byte d'un MyImage dans un fichier sur l'ordinateur au nom de "file"

        /// <summary>
        /// Convertit un tableau d'octet sous format endian en une valeur entière
        /// </summary>
        /// <param name="tab">Le tableau d'octet au format endian</param>
        /// <returns>La valeur de tab sous forme de valeur entière</returns>
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

        /// <summary>
        /// Convertit une valeur entière en un tableau d'octets décrivant un format endian
        /// </summary>
        /// <param name="val">La valeur entière à convertir</param>
        /// <returns>Le tableau d'octets correspondant à la valeur entrée au format endian</returns>
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

        /// <summary>
        /// Inverse les couleurs d'une image par rapport au spectre de couleurs (255 - valeur de chaque pixel rouge, vert, et bleu)
        /// </summary>
        /// <returns>L'image avec couleurs inversées</returns>
        public MyImage Inverse()                        //return l'image avec les coleurs inversés en fct du spectre
        {
            MyImage nouvelleImage = new MyImage(this.Myfile);
            for (int i = 0; i < this.Image.GetLength(0); i++)
            {
                for (int j = 0; j < this.Image.GetLength(1); j++)
                {
                    nouvelleImage.Image[i, j].R = (byte)(255 - this.image[i, j].R);
                    nouvelleImage.Image[i, j].G = (byte)(255 - this.image[i, j].G);
                    nouvelleImage.Image[i, j].B = (byte)(255 - this.image[i, j].B);
                }
            }
            return nouvelleImage;
        }

        /// <summary>
        /// Genere 3 nombres aléatoires qui décideront de la couleur du filtre appliqué (pixel rouge / randomR -- pixel vert / randomV -- pixel bleu / randomB)
        /// </summary>
        /// <returns>L'image avec un filtre de couleur aléatoire</returns>
        public MyImage FiltreCouleurAleatoire()
        {
            MyImage nouvelleImage = new MyImage(this.myfile);
            Random aleatoire = new Random();
            int rouge = aleatoire.Next(1, 6);
            int vert = aleatoire.Next(1, 6);
            int bleu = aleatoire.Next(1, 6);
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    nouvelleImage.image[i, j].R = (byte)(this.image[i, j].R / rouge);
                    nouvelleImage.image[i, j].G = (byte)(this.image[i, j].G / vert);
                    nouvelleImage.image[i, j].B = (byte)(this.image[i, j].B / bleu);
                }
            }
            return nouvelleImage;
        }       //return l'image avec un filtre d'une couleur générée aléatoirement

        /// <summary>
        /// Genere un nombre aléatoire qui décidera d'un changement de couleur en échangeant les valeurs des pixels rouges, verts, et bleus entre eux
        /// </summary>
        /// <returns>L'image avec un nouveau jeu de couleurs aléatoire</returns>
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

        /// <summary>
        /// Change pour chaque pixel rouge, vert puis bleu leur valeur en les échangeant entre eux aléatoirement et ce à chaque boucle
        /// </summary>
        /// <returns>L'image avec un effet de saturation semblable à celui visible sur les anciennes télés cathodiques</returns>
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

        /// <summary>
        /// Genere 3 nombres aléatoires pour chaque pixel rouge, vert, puis bleu, qui décideront de la couleur du filtre appliqué (pixel rouge / randomR -- pixel vert / randomV -- pixel bleu / randomB)
        /// </summary>
        /// <returns>L'image avec un effet de saturation semblable à celui visible sur les anciennes télés cathodiques mais avec des couleurs</returns>
        public MyImage SaturageCouleurs()
        {
            MyImage nouvelleImage = new MyImage(this.myfile);
            Random aleatoire = new Random();
            int rouge;
            int vert;
            int bleu;
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    rouge = aleatoire.Next(1, 6);
                    vert = aleatoire.Next(1, 6);
                    bleu = aleatoire.Next(1, 6);
                    nouvelleImage.image[i, j].R = (byte)(this.image[i, j].R / rouge);
                    nouvelleImage.image[i, j].G = (byte)(this.image[i, j].G / vert);
                    nouvelleImage.image[i, j].B = (byte)(this.image[i, j].B / bleu);
                }
            }
            return nouvelleImage;
        }           //return l'image avec un effet saturé coloré

        /// <summary>
        /// Fais la moyenne des valeurs de rouge, vert et bleu pour chaque pixel et donne à chaque pixel cette valeur
        /// </summary>
        /// <returns>L'image en nuance de gris</returns>
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

        /// <summary>
        /// Calcule si la moyenne des valeurs de rouge, vert et bleu est inférieure à 128 (256/2). Si elle l'est, le pixel devient noir, sinon il devient blanc
        /// </summary>
        /// <returns>L'image uniquement en noir et blanc</returns>
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

        /// <summary>
        /// Construit l'image de droite à gauche pour lui donner un effet miroir horizontal
        /// </summary>
        /// <returns>L'image avec un effet miroir horizontal</returns>
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

        /// <summary>
        /// Construit l'image de haut en bas pour lui donner un effet miroir vertical
        /// </summary>
        /// <returns>L'image avec un effet miroir vertical</returns>
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

        /// <summary>
        /// Fait tourner l'image d'un angle donné en paramètre en utilisant les coordonnées polaires
        /// </summary>
        /// <param name="angleDonne">L'angle duquel l'image va être tournée dans le sens horaire</param>
        /// <returns>L'image tournée de l'angle donné dans le sens horaire</returns>
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

            for (int x = 0; x < coef; x++)
            {
                imageATourner = imageATourner.Rotation90Droite();
            }

            angleRadian = (double)((angleDegre - coef * 90) * Math.PI / 180);
            newHauteur = (int)Math.Abs((imageATourner.hauteurImage * Math.Cos(angleRadian)) + Math.Abs(imageATourner.largeurImage * Math.Sin(angleRadian))) + 1;
            newLargeur = (int)(Math.Abs(imageATourner.largeurImage * Math.Cos(angleRadian)) + Math.Abs(imageATourner.hauteurImage * Math.Cos((Math.PI / 2) - angleRadian))) + 1;
            newTaille = newHauteur * newLargeur * 3 + 54;
            newImage = new Pixel[newHauteur, newLargeur];
            for (int i = 0; i < newHauteur; i++) for (int j = 0; j < newLargeur; j++) newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 0; i < imageATourner.hauteurImage; i++)
            {
                for (int j = 0; j < imageATourner.largeurImage; j++)
                {
                    newImage[(int)(i * Math.Cos(angleRadian) - j * Math.Sin(angleRadian) + imageATourner.largeurImage * Math.Abs(Math.Sin(angleRadian))), (int)Math.Abs(i * Math.Sin(angleRadian) + j * Math.Cos(angleRadian))] = imageATourner.image[i, j];
                }
            }

            pixelColor = true;
            sommeR = 0;
            sommeG = 0;
            sommeB = 0;
            for (int i = 1; i < newHauteur - 1; i++)
            {
                for (int j = 1; j < newLargeur - 1; j++)
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

        /// <summary>
        /// Tourne précisémment l'image de 90 degrés vers la droite
        /// </summary>
        /// <returns>L'image tournée de 90 degrés dans le sens horaire</returns>
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

        /// <summary>
        /// Tourne précisément l'image de 90 degrés vers la gauche
        /// </summary>
        /// <returns>L'image tournée de 90 degrés dans le sens anti-horaire</returns>
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

        /// <summary>
        /// Réduit l'image de coefficients choisis en paramètre en ne prenant qu'un pixel sur -coefHauteur- et -coefLargeur-
        /// </summary>
        /// <param name="coefHauteur">Coefficient de réduction sur la hauteur</param>
        /// <param name="coefLargeur">Coefficient de réduction sur la largeur</param>
        /// <returns>L'image réduite de -coefHauteur- sur la hauteur, et de -coefLargeur- sur la largeur</returns>
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

        /// <summary>
        /// Réduit l'image de coefficients choisis en paramètre en répétant le même pixel -coefHauteur- fois sur la hauteur et -coefLargeur- fois sur la largeur
        /// </summary>
        /// <param name="coefHauteur">Coefficient d'agrandissement sur la hauteur</param>
        /// <param name="coefLargeur">Coefficient d'agrandissement sur la largeur</param>
        /// <returns>L'image agrandie de -coefHauteur- sur la hauteur, et de -coefLargeur- sur la largeur</returns>
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

        /// <summary>
        /// Donne un léger aspect flou à l'image (peu visible sur les grandes images)
        /// </summary>
        /// <returns>L'image avec un effet flou</returns>
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

        /// <summary>
        /// Applique des couleurs très flashy de tout le spectre tout en conservant les formes de l'image (rend moins bien sur les grandes images)
        /// </summary>
        /// <returns>L'image avec un effet psychédélique</returns>
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

        /// <summary>
        /// Détecte les contours de l'image et ne laisse plus qu'eux apparents sur l'image
        /// </summary>
        /// <returns>L'image en noir et blanc avec uniquement les contours visibles</returns>
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

        /// <summary>
        /// Augmente les contrastes de l'image et la rend plus lumineuse, avec des couleurs plus puissantes
        /// </summary>
        /// <returns>L'image avec les contrastes augmentés</returns>
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

        /// <summary>
        /// Renforce les bords en balayant l'image par la verticale
        /// </summary>
        /// <returns>L'image avec un renforcement des bords sur la verticale</returns>
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

        /// <summary>
        /// Renforce les bords en balayant l'image par l'horizontale
        /// </summary>
        /// <returns>L'image avec un renforcement des bords sur l'horizontale</returns>
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

        /// <summary>
        /// Donne un semblant de 3D à l'image en repoussant ses éléments vers l'extérieur de l'écran
        /// </summary>
        /// <returns>L'image avec un effet de repoussage</returns>
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

        /// <summary>
        /// Applique à l'image le filtre de Sobel, qui correspond à une détéction de contours avec couleurs
        /// </summary>
        /// <returns>L'image avec un filtre de Sobel appliqué</returns>
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

        /// <summary>
        /// Applique à l'image une matrice de convolution que nous avons inventé en essayant des valeurs aléatoirement, et donne un semblant de flou en colorant les contours en blanc
        /// </summary>
        /// <returns>L'image une fois que l'on a appliqué une matrice de convolution personnalisée</returns>
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

        /// <summary>
        /// Renvoie l'histogramme de notre image, divisé en 3 courbes pour les valeurs des pixels rouges, verts, et bleus
        /// </summary>
        /// <returns>L'histogramme de l'image</returns>
        public MyImage Histogramme()
        {
            Pixel[,] newImage = new Pixel[300, 768];
            for (int i = 0; i < 300; i++) for (int j = 0; j < 768; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int[] tabCouleurR = new int[256];
            int[] tabCouleurG = new int[256];
            int[] tabCouleurB = new int[256];
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
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
            for (int j = 0; j < 768; j += 3)
            {
                for (int x = 0; x < (tabCouleurR[j / 3] * 300 / maxR); x++)
                {
                    newImage[x, j].R = 255;
                    newImage[x, j + 1].R = 255;
                    newImage[x, j + 2].R = 255;
                }
                for (int x = 0; x < (tabCouleurG[j / 3] * 300 / maxG); x++)
                {
                    newImage[x, j].G = 255;
                    newImage[x, j + 1].G = 255;
                    newImage[x, j + 2].G = 255;
                }
                for (int x = 0; x < (tabCouleurB[j / 3] * 300 / maxB); x++)
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

        /// <summary>
        /// Applique un filtre sur l'image qui impose un dégradé multicolore en diagonale, tout en gardant les formes de l'image originelle
        /// </summary>
        /// <returns>L'image avec un filtre de dégradé multicolore</returns>
        public MyImage DegradeMulticolore()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    newImage[i, j].R = (byte)Math.Abs(this.image[i, j].R - i + j);
                    newImage[i, j].G = (byte)Math.Abs(this.image[i, j].G + 2 * i - 2 * j);
                    newImage[i, j].B = (byte)Math.Abs(this.image[i, j].B - i + j);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                       //return l'image avec un filtre de dégradé multicolore

        /// <summary>
        /// Crée un axe de symétrie horizontal au milieu de l'image
        /// </summary>
        /// <returns>L'image avec une symétrie suivant l'axe horizontal du milieu de l'image</returns>
        public MyImage SymetrieHorizontale()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int k = this.hauteurImage / 2;
            for (int i = 0; i < this.hauteurImage / 2; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[i, j];
                }
            }
            for (int i = this.hauteurImage / 2; i < this.hauteurImage; i++)
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

        /// <summary>
        /// Crée un axe de symétrie vertical au milieu de l'image
        /// </summary>
        /// <returns>L'image avec une symétrie suivant l'axe vertical du milieu de l'image</returns>
        public MyImage SymetrieVerticale()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            int k = this.largeurImage / 2;
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage / 2; j++)
                {
                    newImage[i, j] = this.image[i, j];
                }
            }
            for (int i = 0; i < this.hauteurImage; i++)
            {
                k = this.largeurImage / 2;
                for (int j = this.largeurImage / 2; j < this.largeurImage; j++)
                {
                    newImage[i, j] = this.image[i, k];
                    k--;
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                   //return l'image avec une symétrie sur un axe vertical

        /// <summary>
        /// Crée deux axes de symétries, un horizontal et un vertical passant par le milieu de l'image
        /// </summary>
        /// <returns>L'image avec une symétrie suivant ces deux axes</returns>
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

        /// <summary>
        /// Convertit un byte en sa valeur binaire sur 8 bits sous la forme d'un tableau de int
        /// </summary>
        /// <param name="valeur">La valeur en byte à convertir en binaire sur 8 bits</param>
        /// <returns>La valeur en byte convertie en binaire sur 8 bits</returns>
        public static int[] Convert_Byte_To_Binary(byte valeur)
        {
            int[] tabHexadecimal = new int[8];
            for (int i = 0; i < 8; i++)
            {
                tabHexadecimal[i] = (byte)(valeur / Math.Pow(2, 7 - i));
                valeur = (byte)(valeur % Math.Pow(2, 7 - i));
            }
            return tabHexadecimal;
        }           //return le byte entré en un tableau de int correspondant aux valeurs binaires de la valeur sur 8 bits

        /// <summary>
        /// Convertit une valeur binaire sur 8 bits en un byte
        /// </summary>
        /// <param name="tabHexadecimal">Valeur binaire à convertir en byte</param>
        /// <returns>La valeur binaire convertie en byte</returns>
        public static byte Convert_Binary_To_Byte(int[] tabBinaire)
        {
            byte valeur = 0;
            for (int i = 0; i < 8; i++)
            {
                valeur += (byte)(tabBinaire[i] * Math.Pow(2, 7 - i));
            }
            return valeur;
        }           //return la valeur entière d'un nombre en binaire sur 8 bits

        /// <summary>
        /// Cache une image dans l'image étudiée en partant de son coin bas gauche
        /// </summary>
        /// <param name="nomImage2">Le nom du fichier de l'image à cacher dans l'image étudiée</param>
        /// <returns>L'image étudiée avec l'image "nomImage2" cachée dedans</returns>
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
                    int[] hexaImage1R = Convert_Byte_To_Binary(this.image[i, j].R);
                    int[] hexaImage2R = Convert_Byte_To_Binary(image2.image[i, j].R);
                    int[] newHexaR = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaR[k] = hexaImage1R[k];
                        else newHexaR[k] = hexaImage2R[k - 4];
                    }
                    newImage[i, j].R = Convert_Binary_To_Byte(newHexaR);

                    int[] hexaImage1G = Convert_Byte_To_Binary(this.image[i, j].G);
                    int[] hexaImage2G = Convert_Byte_To_Binary(image2.image[i, j].G);
                    int[] newHexaG = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaG[k] = hexaImage1G[k];
                        else newHexaG[k] = hexaImage2G[k - 4];
                    }
                    newImage[i, j].G = Convert_Binary_To_Byte(newHexaG);

                    int[] hexaImage1B = Convert_Byte_To_Binary(this.image[i, j].B);
                    int[] hexaImage2B = Convert_Byte_To_Binary(image2.image[i, j].B);
                    int[] newHexaB = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaB[k] = hexaImage1B[k];
                        else newHexaB[k] = hexaImage2B[k - 4];
                    }
                    newImage[i, j].B = Convert_Binary_To_Byte(newHexaB);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                   //return une image avec une autre cachée dedans

        /// <summary>
        /// Retrouve une image qui a été cachée dans une autre
        /// </summary>
        /// <returns>L'image cachée en passant les bits de poids faible de chaque pixels en bits de poids forts</returns>
        public MyImage RetrouverImage()
        {
            Pixel[,] newImage = new Pixel[this.hauteurImage, this.largeurImage];
            for (int i = 0; i < this.hauteurImage; i++) for (int j = 0; j < this.largeurImage; j++) newImage[i, j] = new Pixel(0, 0, 0);
            for (int i = 0; i < this.hauteurImage; i++)
            {
                for (int j = 0; j < this.largeurImage; j++)
                {
                    int[] hexaImageR = Convert_Byte_To_Binary(this.image[i, j].R);
                    int[] newHexaR = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaR[k] = hexaImageR[k + 4];
                        else newHexaR[k] = 0;
                    }
                    newImage[i, j].R = Convert_Binary_To_Byte(newHexaR);

                    int[] hexaImageG = Convert_Byte_To_Binary(this.image[i, j].G);
                    int[] newHexaG = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaG[k] = hexaImageG[k + 4];
                        else newHexaG[k] = 0;
                    }
                    newImage[i, j].G = Convert_Binary_To_Byte(newHexaG);

                    int[] hexaImageB = Convert_Byte_To_Binary(this.image[i, j].B);
                    int[] newHexaB = new int[8];
                    for (int k = 0; k < 8; k++)
                    {
                        if (k < 4) newHexaB[k] = hexaImageB[k + 4];
                        else newHexaB[k] = 0;
                    }
                    newImage[i, j].B = Convert_Binary_To_Byte(newHexaB);
                }
            }
            MyImage nouvelleImage = new MyImage("BitMap", this.tailleFichier, this.tailleOffset, this.hauteurImage, this.largeurImage, this.nbBitsCouleur, newImage);
            return nouvelleImage;
        }                                   //return l'image cachée derrière celle fabriquée

        /// <summary>
        /// Dessine l'image dans la console sous forme d'Ascii Art, c'est-à-dire avec des caractères
        /// </summary>
        public void Ascii()                                         //return dans la console l'image en ascii art
        {
            int moyenne;
            int coef = (this.largeurImage / 211) + 1;
            for (int i = this.hauteurImage - 1; i >= 0; i -= coef * 2)
            {
                Console.WriteLine();
                for (int j = 0; j < this.largeurImage; j += coef)
                {
                    moyenne = (this.image[i, j].R + this.image[i, j].G + this.image[i, j].B) / 3;
                    switch (moyenne / 10)
                    {
                        case 0:
                            Console.Write(" ");
                            break;
                        case 1:
                            Console.Write(".");
                            break;
                        case 2:
                            Console.Write("'");
                            break;
                        case 3:
                            Console.Write("-");
                            break;
                        case 4:
                            Console.Write("_");
                            break;
                        case 5:
                            Console.Write("^");
                            break;
                        case 6:
                            Console.Write(":");
                            break;
                        case 7:
                            Console.Write(";");
                            break;
                        case 8:
                            Console.Write("!");
                            break;
                        case 9:
                            Console.Write('"');
                            break;
                        case 10:
                            Console.Write("*");
                            break;
                        case 11:
                            Console.Write("c");
                            break;
                        case 12:
                            Console.Write("+");
                            break;
                        case 13:
                            Console.Write("=");
                            break;
                        case 14:
                            Console.Write("/");
                            break;
                        case 15:
                            Console.Write("?");
                            break;
                        case 16:
                            Console.Write("€");
                            break;
                        case 17:
                            Console.Write("£");
                            break;
                        case 18:
                            Console.Write("$");
                            break;
                        case 19:
                            Console.Write("%");
                            break;
                        case 20:
                            Console.Write("&");
                            break;
                        case 21:
                            Console.Write("§");
                            break;
                        case 22:
                            Console.Write("#");
                            break;
                        case 23:
                            Console.Write("B");
                            break;
                        case 24:
                            Console.Write("@");
                            break;
                    }
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Dessine l'image dans la console sous forme d'Ascii Art, c'est-à-dire avec des caractères, mais avec des couleurs
        /// </summary>
        public void AsciiCouleurs()                                    //return dans la console l'image en ascii art avec des couleurs
        {
            int moyenne;
            int coef = (this.largeurImage / 211) + 1;
            for (int i = this.hauteurImage - 1; i >= 0; i -= coef * 2)
            {
                Console.WriteLine();
                for (int j = 0; j < this.largeurImage; j += coef)
                {
                    if (this.image[i, j].R >= this.image[i, j].G && this.image[i, j].G <= this.image[i, j].B * 2 && this.image[i, j].B < this.image[i, j].R) Console.ForegroundColor = ConsoleColor.Red;
                    if (this.image[i, j].G >= this.image[i, j].B && this.image[i, j].B <= this.image[i, j].R * 2 && this.image[i, j].R < this.image[i, j].G) Console.ForegroundColor = ConsoleColor.Green;
                    if (this.image[i, j].B >= this.image[i, j].R && this.image[i, j].R <= this.image[i, j].G * 2 && this.image[i, j].G < this.image[i, j].B) Console.ForegroundColor = ConsoleColor.Blue;
                    if (this.image[i, j].R >= this.image[i, j].G && this.image[i, j].G <= this.image[i, j].B * 2 && this.image[i, j].B < this.image[i, j].R && this.image[i, j].R < 100) Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (this.image[i, j].G >= this.image[i, j].B && this.image[i, j].B <= this.image[i, j].R * 2 && this.image[i, j].R < this.image[i, j].G && this.image[i, j].G < 100) Console.ForegroundColor = ConsoleColor.DarkGreen;
                    if (this.image[i, j].B >= this.image[i, j].R && this.image[i, j].R <= this.image[i, j].G * 2 && this.image[i, j].G < this.image[i, j].B && this.image[i, j].B < 100) Console.ForegroundColor = ConsoleColor.DarkBlue;
                    if (this.image[i, j].R >= this.image[i, j].G && this.image[i, j].G > this.image[i, j].B * 2) Console.ForegroundColor = ConsoleColor.Yellow;
                    if (this.image[i, j].G >= this.image[i, j].R && this.image[i, j].R > this.image[i, j].B * 2) Console.ForegroundColor = ConsoleColor.Yellow;
                    if (this.image[i, j].R >= this.image[i, j].G && this.image[i, j].G > this.image[i, j].B * 2 && this.image[i, j].R < 100) Console.ForegroundColor = ConsoleColor.DarkYellow;
                    if (this.image[i, j].G >= this.image[i, j].R && this.image[i, j].R > this.image[i, j].B * 2 && this.image[i, j].G < 100) Console.ForegroundColor = ConsoleColor.DarkYellow;
                    if (this.image[i, j].G >= this.image[i, j].B && this.image[i, j].B > this.image[i, j].R * 2) Console.ForegroundColor = ConsoleColor.Cyan;
                    if (this.image[i, j].B >= this.image[i, j].G && this.image[i, j].G > this.image[i, j].R * 2) Console.ForegroundColor = ConsoleColor.Cyan;
                    if (this.image[i, j].G >= this.image[i, j].B && this.image[i, j].B > this.image[i, j].R * 2 && this.image[i, j].G < 100) Console.ForegroundColor = ConsoleColor.DarkCyan;
                    if (this.image[i, j].B >= this.image[i, j].G && this.image[i, j].G > this.image[i, j].R * 2 && this.image[i, j].B < 100) Console.ForegroundColor = ConsoleColor.DarkCyan;
                    if (this.image[i, j].B >= this.image[i, j].R && this.image[i, j].R > this.image[i, j].G * 2) Console.ForegroundColor = ConsoleColor.Magenta;
                    if (this.image[i, j].R >= this.image[i, j].B && this.image[i, j].B > this.image[i, j].G * 2) Console.ForegroundColor = ConsoleColor.Magenta;
                    if (this.image[i, j].B >= this.image[i, j].R && this.image[i, j].R > this.image[i, j].G * 2 && this.image[i, j].B < 100) Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    if (this.image[i, j].R >= this.image[i, j].B && this.image[i, j].B > this.image[i, j].G * 2 && this.image[i, j].R < 100) Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    if (this.image[i, j].R >= 190 && this.image[i, j].G >= 190 && this.image[i, j].B >= 190) Console.ForegroundColor = ConsoleColor.White;
                    if (this.image[i, j].R <= 50 && this.image[i, j].G <= 50 && this.image[i, j].B <= 50) Console.ForegroundColor = ConsoleColor.DarkGray;
                    else if (Math.Abs(this.image[i, j].R - this.image[i, j].G) < 10 && Math.Abs(this.image[i, j].R - this.image[i, j].B) < 10 && Math.Abs(this.image[i, j].B - this.image[i, j].G) < 10) Console.ForegroundColor = ConsoleColor.Gray;

                    moyenne = (this.image[i, j].R + this.image[i, j].G + this.image[i, j].B) / 3;
                    switch (moyenne / 10)
                    {
                        case 0:
                            Console.Write(" ");
                            break;
                        case 1:
                            Console.Write(".");
                            break;
                        case 2:
                            Console.Write("'");
                            break;
                        case 3:
                            Console.Write("-");
                            break;
                        case 4:
                            Console.Write("_");
                            break;
                        case 5:
                            Console.Write("^");
                            break;
                        case 6:
                            Console.Write(":");
                            break;
                        case 7:
                            Console.Write(";");
                            break;
                        case 8:
                            Console.Write("!");
                            break;
                        case 9:
                            Console.Write('"');
                            break;
                        case 10:
                            Console.Write("*");
                            break;
                        case 11:
                            Console.Write("c");
                            break;
                        case 12:
                            Console.Write("+");
                            break;
                        case 13:
                            Console.Write("=");
                            break;
                        case 14:
                            Console.Write("/");
                            break;
                        case 15:
                            Console.Write("?");
                            break;
                        case 16:
                            Console.Write("€");
                            break;
                        case 17:
                            Console.Write("£");
                            break;
                        case 18:
                            Console.Write("$");
                            break;
                        case 19:
                            Console.Write("%");
                            break;
                        case 20:
                            Console.Write("&");
                            break;
                        case 21:
                            Console.Write("§");
                            break;
                        case 22:
                            Console.Write("#");
                            break;
                        case 23:
                            Console.Write("B");
                            break;
                        case 24:
                            Console.Write("@");
                            break;
                    }
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Dessine la fractale de Mandelbrot de taille [hauteur x largeur], et de couleur dépendant des valeurs de coefR, coefG, et coefB
        /// </summary>
        /// <param name="hauteur">Hauteur de la fractale</param>
        /// <param name="largeur">Largeur de la fractale</param>
        /// <param name="coefR">Intensité de la couleur rouge</param>
        /// <param name="coefG">Intensité de la couleur verte</param>
        /// <param name="coefB">Intensité de la couleur bleue</param>
        /// <returns>La fractale de Mandelbrot selon les dimensions et couleurs précisées en paramètres</returns>
        public static MyImage FractaleMandelbrot(int hauteur, int largeur, double coefR, double coefG, double coefB)
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int n = 0; n < hauteur; n++) for (int m = 0; m < largeur; m++) newImage[n, m] = new Pixel(0, 0, 0);
            double borneGauche = -1.5;
            double borneDroite = 0.6;
            double borneHaut = 1;
            double borneBas = -1;

            //int zoom = 100;
            int iteration_max = 50;

            double cooReel;         //coo du point associé
            double cooImaginaire;
            double xn;              //termes de la suite
            double yn;
            int i = 0;
            double tmp_x = 0;           // xn - 1
            double tmp_y = 0;           //yn -1
            for (int x = 0; x < hauteur; x++)
            {
                for (int y = 0; y < largeur; y++)
                {
                    cooReel = ((y * (borneDroite - borneGauche) / largeur) + borneGauche);    //remise à l'échelle pour que les pixels de l'image soient associés à un point du plan
                    cooImaginaire = ((x * (borneHaut - borneBas) / hauteur) + borneBas);
                    xn = 0;
                    yn = 0;
                    i = 0;

                    while ((xn * xn + yn * yn) < 4 && i < iteration_max)        //il est admis que si xn² + yn² >4, la suite DV vers l'infini
                    {
                        tmp_x = xn; //stockage xn -1 et yn - 1
                        tmp_y = yn;
                        //application de la suite terme(n+1) = terme(n)² + point
                        xn = tmp_x * tmp_x - tmp_y * tmp_y + cooReel;
                        yn = 2 * tmp_x * tmp_y + cooImaginaire;
                        i++;
                    }
                    //si xn² + yn² >4 avant l'iteration max, la suite DV et on colorie
                    if (i != iteration_max) newImage[x, y] = new Pixel((byte)((coefR * i) % 256), (byte)((coefG * i) % 256), (byte)((coefB * i) % 256));
                }
            }
            int tailleFichier = hauteur * largeur * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, hauteur, largeur, 24, newImage);
            return nouvelleImage;
        }   //return la fractale de mandelbrot de taille [hauteur,largeur] et d'intensité de couleurs (coefR, coefG, coefB)

        /// <summary>
        /// Dessine une fractale de Julia de taille [hauteur x largeur], et de couleur dépendant des valeurs de coefR, coefG, et coefB
        /// </summary>
        /// <param name="hauteur">Hauteur de la fractale</param>
        /// <param name="largeur">Largeur de la fractale</param>
        /// <param name="coefR">Intensité de la couleur rouge</param>
        /// <param name="coefG">Intensité de la couleur verte</param>
        /// <param name="coefB">Intensité de la couleur bleue</param>
        /// <returns>Une fractale de Julia selon les dimensions et couleurs précisées en paramètres</returns>
        public static MyImage FractaleJulia1(int hauteur, int largeur, double coefR, double coefG, double coefB)
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int n = 0; n < hauteur; n++) for (int m = 0; m < largeur; m++) newImage[n, m] = new Pixel(0, 0, 0);
            double borneGauche = -1.3;
            double borneDroite = 1.3;
            double borneHaut = 1;
            double borneBas = -1;

            //int zoom = 100;
            int iteration_max = 100;

            double cooReel;         //coo du point associé
            double cooImaginaire;
            double xn;              //termes de la suite
            double yn;
            int i = 0;
            double tmp_x = 0;           // xn - 1
            double tmp_y = 0;           //yn -1
            for (int x = 0; x < hauteur; x++)
            {
                for (int y = 0; y < largeur; y++)
                {
                    cooReel = ((y * (borneDroite - borneGauche) / largeur) + borneGauche);    //remise à l'échelle pour que les pixels de l'image soient associés à un point du plan
                    cooImaginaire = ((x * (borneHaut - borneBas) / hauteur) + borneBas);
                    xn = cooReel;
                    yn = cooImaginaire;
                    i = 0;

                    while ((xn * xn + yn * yn) < 20 && i < iteration_max)        //il est admis que si xn² + yn² >4, la suite DV vers l'infini
                    {
                        tmp_x = xn; //stockage xn -1 et yn - 1
                        tmp_y = yn;
                        //application de la suite terme(n+1) = terme(n)² + point
                        xn = tmp_x * tmp_x - tmp_y * tmp_y - 0.39;
                        yn = 2 * tmp_x * tmp_y - 0.59;
                        i++;
                    }
                    //si xn² + yn² >4 avant l'iteration max, la suite DV et on colorie
                    if (i != iteration_max) newImage[x, y] = new Pixel((byte)((coefR * i) % 256), (byte)((coefG * i) % 256), (byte)((coefB * i) % 256));
                }
            }
            int tailleFichier = hauteur * largeur * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, hauteur, largeur, 24, newImage);
            return nouvelleImage;
        }   //return une fractale de Julia de taille [hauteur,largeur] et d'intensité de couleurs (coefR, coefG, coefB)

        /// <summary>
        /// Dessine une fractale de Julia de taille [hauteur x largeur], et de couleur dépendant des valeurs de coefR, coefG, et coefB
        /// </summary>
        /// <param name="hauteur">Hauteur de la fractale</param>
        /// <param name="largeur">Largeur de la fractale</param>
        /// <param name="coefR">Intensité de la couleur rouge</param>
        /// <param name="coefG">Intensité de la couleur verte</param>
        /// <param name="coefB">Intensité de la couleur bleue</param>
        /// <returns>Une fractale de Julia selon les dimensions et couleurs précisées en paramètres</returns>
        public static MyImage FractaleJulia2(int hauteur, int largeur, double coefR, double coefG, double coefB)
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int n = 0; n < hauteur; n++) for (int m = 0; m < largeur; m++) newImage[n, m] = new Pixel(0, 0, 0);
            double borneGauche = -1.5;
            double borneDroite = 1.5;
            double borneHaut = 1;
            double borneBas = -1;

            //int zoom = 100;
            int iteration_max = 100;

            double cooReel;         //coo du point associé
            double cooImaginaire;
            double xn;              //termes de la suite
            double yn;
            int i = 0;
            double tmp_x = 0;           // xn - 1
            double tmp_y = 0;           //yn -1
            for (int x = 0; x < hauteur; x++)
            {
                for (int y = 0; y < largeur; y++)
                {
                    cooReel = ((y * (borneDroite - borneGauche) / largeur) + borneGauche);    //remise à l'échelle pour que les pixels de l'image soient associés à un point du plan
                    cooImaginaire = ((x * (borneHaut - borneBas) / hauteur) + borneBas);
                    xn = cooReel;
                    yn = cooImaginaire;
                    i = 0;

                    while ((xn * xn + yn * yn) < 20 && i < iteration_max)        //il est admis que si xn² + yn² >4, la suite DV vers l'infini
                    {
                        tmp_x = xn; //stockage xn -1 et yn - 1
                        tmp_y = yn;
                        //application de la suite terme(n+1) = terme(n)² + point
                        xn = tmp_x * tmp_x - tmp_y * tmp_y - 0.79;
                        yn = 2 * tmp_x * tmp_y + 0.15;
                        i++;
                    }
                    //si xn² + yn² >4 avant l'iteration max, la suite DV et on colorie
                    if (i != iteration_max) newImage[x, y] = new Pixel((byte)((coefR * i) % 256), (byte)((coefG * i) % 256), (byte)((coefB * i) % 256));
                }
            }
            int tailleFichier = hauteur * largeur * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, hauteur, largeur, 24, newImage);
            return nouvelleImage;
        }   //return une fractale de Julia de taille [hauteur,largeur] et d'intensité de couleurs (coefR, coefG, coefB)

        /// <summary>
        /// Dessine une fractale de Julia de taille [hauteur x largeur], et de couleur dépendant des valeurs de coefR, coefG, et coefB
        /// </summary>
        /// <param name="hauteur">Hauteur de la fractale</param>
        /// <param name="largeur">Largeur de la fractale</param>
        /// <param name="coefR">Intensité de la couleur rouge</param>
        /// <param name="coefG">Intensité de la couleur verte</param>
        /// <param name="coefB">Intensité de la couleur bleue</param>
        /// <returns>Une fractale de Julia selon les dimensions et couleurs précisées en paramètres</returns>
        public static MyImage FractaleJulia3(int hauteur, int largeur, double coefR, double coefG, double coefB)
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int n = 0; n < hauteur; n++) for (int m = 0; m < largeur; m++) newImage[n, m] = new Pixel(0, 0, 0);
            double borneGauche = -1.5;
            double borneDroite = 1.5;
            double borneHaut = 1;
            double borneBas = -1;

            //int zoom = 100;
            int iteration_max = 100;

            double cooReel;         //coo du point associé
            double cooImaginaire;
            double xn;              //termes de la suite
            double yn;
            int i = 0;
            double tmp_x = 0;           // xn - 1
            double tmp_y = 0;           //yn -1
            for (int x = 0; x < hauteur; x++)
            {
                for (int y = 0; y < largeur; y++)
                {
                    cooReel = ((y * (borneDroite - borneGauche) / largeur) + borneGauche);    //remise à l'échelle pour que les pixels de l'image soient associés à un point du plan
                    cooImaginaire = ((x * (borneHaut - borneBas) / hauteur) + borneBas);
                    xn = cooReel;
                    yn = cooImaginaire;
                    i = 0;

                    while ((xn * xn + yn * yn) < 20 && i < iteration_max)        //il est admis que si xn² + yn² >4, la suite DV vers l'infini
                    {
                        tmp_x = xn; //stockage xn -1 et yn - 1
                        tmp_y = yn;
                        //application de la suite terme(n+1) = terme(n)² + point
                        xn = tmp_x * tmp_x - tmp_y * tmp_y - 0.7;
                        yn = 2 * tmp_x * tmp_y + 0.27015;
                        i++;
                    }
                    //si xn² + yn² >4 avant l'iteration max, la suite DV et on colorie
                    if (i != iteration_max) newImage[x, y] = new Pixel((byte)((coefR * i) % 256), (byte)((coefG * i) % 256), (byte)((coefB * i) % 256));
                }
            }
            int tailleFichier = hauteur * largeur * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, hauteur, largeur, 24, newImage);
            return nouvelleImage;
        }   //return une fractale de Julia de taille [hauteur,largeur] et d'intensité de couleurs (coefR, coefG, coefB)

        /// <summary>
        /// Dessine une fractale de Julia de taille [hauteur x largeur], et de couleur dépendant des valeurs de coefR, coefG, et coefB
        /// </summary>
        /// <param name="hauteur">Hauteur de la fractale</param>
        /// <param name="largeur">Largeur de la fractale</param>
        /// <param name="coefR">Intensité de la couleur rouge</param>
        /// <param name="coefG">Intensité de la couleur verte</param>
        /// <param name="coefB">Intensité de la couleur bleue</param>
        /// <returns>Une fractale de Julia selon les dimensions et couleurs précisées en paramètres</returns>
        public static MyImage FractaleJulia4(int hauteur, int largeur, double coefR, double coefG, double coefB)
        {
            Pixel[,] newImage = new Pixel[hauteur, largeur];
            for (int n = 0; n < hauteur; n++) for (int m = 0; m < largeur; m++) newImage[n, m] = new Pixel(0, 0, 0);
            double borneGauche = -1.3;
            double borneDroite = 1.3;
            double borneHaut = 1.3;
            double borneBas = -1.3;

            //int zoom = 100;
            int iteration_max = 100;

            double cooReel;         //coo du point associé
            double cooImaginaire;
            double xn;              //termes de la suite
            double yn;
            int i = 0;
            double tmp_x = 0;           // xn - 1
            double tmp_y = 0;           //yn -1
            for (int x = 0; x < hauteur; x++)
            {
                for (int y = 0; y < largeur; y++)
                {
                    cooReel = ((y * (borneDroite - borneGauche) / largeur) + borneGauche);    //remise à l'échelle pour que les pixels de l'image soient associés à un point du plan
                    cooImaginaire = ((x * (borneHaut - borneBas) / hauteur) + borneBas);
                    xn = cooReel;
                    yn = cooImaginaire;
                    i = 0;

                    while ((xn * xn + yn * yn) < 20 && i < iteration_max)        //il est admis que si xn² + yn² >4, la suite DV vers l'infini
                    {
                        tmp_x = xn; //stockage xn -1 et yn - 1
                        tmp_y = yn;
                        //application de la suite terme(n+1) = terme(n)² + point
                        xn = tmp_x * tmp_x - tmp_y * tmp_y + 0.285;
                        yn = 2 * tmp_x * tmp_y + 0.01;
                        i++;
                    }
                    //si xn² + yn² >4 avant l'iteration max, la suite DV et on colorie
                    if (i != iteration_max) newImage[x, y] = new Pixel((byte)((coefR * i) % 256), (byte)((coefG * i) % 256), (byte)((coefB * i) % 256));
                }
            }
            int tailleFichier = hauteur * largeur * 3 + 54;
            int tailleOffset = tailleFichier - 54;
            MyImage nouvelleImage = new MyImage("BitMap", tailleFichier, tailleOffset, hauteur, largeur, 24, newImage);
            return nouvelleImage;
        }   //return une fractale de Julia de taille [hauteur,largeur] et d'intensité de couleurs (coefR, coefG, coefB)

        /// <summary>
        /// Convertit certains char en leur valeur alphanumérique
        /// </summary>
        /// <param name="lettre">Le char à convertir</param>
        /// <returns>La valeur alphanumérique du char à convertir</returns>
        public static int ConvertCharToAlphanum(char lettre)
        {
            int val = -1;
            switch (lettre)
            {
                case '0':
                    val = 0;
                    break;
                case '1':
                    val = 1;
                    break;
                case '2':
                    val = 2;
                    break;
                case '3':
                    val = 3;
                    break;
                case '4':
                    val = 4;
                    break;
                case '5':
                    val = 5;
                    break;
                case '6':
                    val = 6;
                    break;
                case '7':
                    val = 7;
                    break;
                case '8':
                    val = 8;
                    break;
                case '9':
                    val = 9;
                    break;
                case 'A':
                    val = 10;
                    break;
                case 'B':
                    val = 11;
                    break;
                case 'C':
                    val = 12;
                    break;
                case 'D':
                    val = 13;
                    break;
                case 'E':
                    val = 14;
                    break;
                case 'F':
                    val = 15;
                    break;
                case 'G':
                    val = 16;
                    break;
                case 'H':
                    val = 17;
                    break;
                case 'I':
                    val = 18;
                    break;
                case 'J':
                    val = 19;
                    break;
                case 'K':
                    val = 20;
                    break;
                case 'L':
                    val = 21;
                    break;
                case 'M':
                    val = 22;
                    break;
                case 'N':
                    val = 23;
                    break;
                case 'O':
                    val = 24;
                    break;
                case 'P':
                    val = 25;
                    break;
                case 'Q':
                    val = 26;
                    break;
                case 'R':
                    val = 27;
                    break;
                case 'S':
                    val = 28;
                    break;
                case 'T':
                    val = 29;
                    break;
                case 'U':
                    val = 30;
                    break;
                case 'V':
                    val = 31;
                    break;
                case 'W':
                    val = 32;
                    break;
                case 'X':
                    val = 33;
                    break;
                case 'Y':
                    val = 34;
                    break;
                case 'Z':
                    val = 35;
                    break;
                case ' ':
                    val = 36;
                    break;
                case '$':
                    val = 37;
                    break;
                case '%':
                    val = 38;
                    break;
                case '*':
                    val = 39;
                    break;
                case '+':
                    val = 40;
                    break;
                case '-':
                    val = 41;
                    break;
                case '.':
                    val = 42;
                    break;
                case '/':
                    val = 43;
                    break;
                case ':':
                    val = 44;
                    break;
                default:
                    break;
            }
            return val;
        }  //return la valeur d'un charactère en alphanumérique

        /// <summary>
        /// Transforme une chaine de 2 char en leur somme de leur valeur alphanumérique en base 45, puis convertit cette somme en sa valeur binaire sur 11 bits (ou 6 bits si -lettres- ne contient qu'un seul char)
        /// </summary>
        /// <param name="lettres">La chaine de 2 char à convertir</param>
        /// <returns>La valeur binaire sur 11 bits de 2 char après conversions en alphanumérique</returns>
        public static int[] Convert2CharTo11Bits(char[] lettres)
        {
            int[] tab11bits = null;
            if (lettres.Length == 2)
            {
                int val1 = ConvertCharToAlphanum(lettres[0]);
                int val2 = ConvertCharToAlphanum(lettres[1]);
                if (val1 != -1 && val2 != -1)
                {
                    int somme = 45 * val1 + val2;
                    tab11bits = new int[11];
                    for (int i = 0; i < 11; i++)
                    {
                        tab11bits[i] = somme / (int)Math.Pow(2, 10 - i);
                        somme = somme % (int)Math.Pow(2, 10 - i);
                    }
                }
            }
            if (lettres.Length == 1)
            {
                int val3 = ConvertCharToAlphanum(lettres[0]);
                if (val3 != -1)
                {
                    tab11bits = new int[6];
                    for (int i = 0; i < 6; i++)
                    {
                        tab11bits[i] = val3 / (int)Math.Pow(2, 5 - i);
                        val3 = val3 % (int)Math.Pow(2, 5 - i);
                    }
                }
            }
            return tab11bits;
        }     //return une chaine de 2 caractères en binaire de 11 bits

        /// <summary>
        /// Convertit une valeur entière en sa valeur binaire sur 9 bits
        /// </summary>
        /// <param name="val">La valeur entière à convertir en binaire</param>
        /// <returns>La valeur entière convertie en binaire sur 9 bits</returns>
        public static int[] ConvertIntTo9Bits(int val)
        {
            int[] tabBinaire = new int[9];
            for (int i = 0; i < 9; i++)
            {
                tabBinaire[i] = (byte)(val / Math.Pow(2, 8 - i));
                val = (byte)(val % Math.Pow(2, 8 - i));
            }
            return tabBinaire;
        }           //Convertit une valeur entière en un tableau de 9 bits

        /// <summary>
        /// Méthode d'ajout de bord blancs pour améliorer la lisibilité du QR code par la caméra
        /// </summary>
        /// <param name="image">Image dont on veut ajouter les bords blancs</param>
        /// <returns>L'image en paramètre avec des bords blancs de 2 pixels de largeur</returns>
        public static Pixel[,] AjoutBordsBlancs(Pixel[,] image)
        {
            Pixel[,] newImage = new Pixel[image.GetLength(0) + 4, image.GetLength(1) + 4];
            for (int i = 0; i < newImage.GetLength(0); i++)
                for (int j = 0; j < newImage.GetLength(1); j++)
                    newImage[i, j] = new Pixel(0, 0, 0);

            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < 2; j++)
                    newImage[i, j] = new Pixel(255, 255, 255);
                for (int j = newImage.GetLength(1) - 2; j < newImage.GetLength(1); j++)
                    newImage[i, j] = new Pixel(255, 255, 255);
            }
            for (int j = 2; j < newImage.GetLength(1) - 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    newImage[i, j] = new Pixel(255, 255, 255);
                }
                for (int i = newImage.GetLength(0) - 2; i < newImage.GetLength(0); i++)
                {
                    newImage[i, j] = new Pixel(255, 255, 255);
                }
            }
            
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    newImage[i + 2, j + 2] = image[i, j];
                }
            }


            return newImage;
        }

        /// <summary>
        /// Construit toutes les parties constantes d'un QR code de niveau 1
        /// </summary>
        /// <param name="imageQR">L'image du QRcode initialisé à la bonne taille permettant de noircir les pixels concernés</param>
        /// <returns>Une matrice de booléens indiquant quelles cases de l'image sont constantes et ne seront donc pas modifiées lors de l'écriture du QR code</returns>
        public static bool[,] ConstructionQRcodeNiveau1(Pixel[,] imageQR)
        {
            bool[,] casesOccupees = new bool[21, 21];
            for (int i = 0; i < 21; i++) for (int j = 0; j < 21; j++) casesOccupees[i, j] = false;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)             //construction motif de recherche bas gauche
                {
                    if (i == 0 || i == 6) imageQR[i, j] = new Pixel(0, 0, 0);
                    else if ((j % 2 == 0 || j == 3) && i != 1 && i != 5) imageQR[i, j] = new Pixel(0, 0, 0);
                    if (j == 0 || j == 6) imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int i = 20; i >= 14; i--)              //construction motif de recherche haut gauche
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i == 20 || i == 14) imageQR[i, j] = new Pixel(0, 0, 0);
                    else if ((j % 2 == 0 || j == 3) && i != 19 && i != 15) imageQR[i, j] = new Pixel(0, 0, 0);
                    if (j == 0 || j == 6) imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int i = 20; i >= 14; i--)              //construction motif de recherche haut droite
            {
                for (int j = 20; j >= 14; j--)
                {
                    if (i == 20 || i == 14) imageQR[i, j] = new Pixel(0, 0, 0);
                    else if ((j % 2 == 0 || j == 17) && i != 19 && i != 15) imageQR[i, j] = new Pixel(0, 0, 0);
                    if (j == 20 || j == 14) imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int x = 8; x < 14; x += 2)                     //construction séparateurs
            {
                imageQR[14, x] = new Pixel(0, 0, 0);
                imageQR[x, 6] = new Pixel(0, 0, 0);
                casesOccupees[14, x] = true;
                casesOccupees[14, x + 1] = true;
                casesOccupees[x, 6] = true;
                casesOccupees[x + 1, 6] = true;
            }
            imageQR[7, 8] = new Pixel(0, 0, 0);
            casesOccupees[7, 8] = true;

            int[] tabNiveauCorrection = { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
            int m1 = 0;
            int m2 = 7;
            for (int j = 0; j < 21; j++)
            {
                if (j <= 8 || j >= 13)
                {
                    if (j <= 8 && j != 6)                   //remplissage des bits composant le niveau de correcteur et le type de masque
                    {
                        if (tabNiveauCorrection[m1] == 1) imageQR[12, j] = new Pixel(0, 0, 0);
                        m1++;
                    }
                    if (j >= 13)
                    {
                        if (tabNiveauCorrection[m2] == 1) imageQR[12, j] = new Pixel(0, 0, 0);
                        m2++;
                    }
                    casesOccupees[12, j] = true;
                }
                if (j <= 7 || j >= 13) casesOccupees[13, j] = true;
                if (j <= 7) casesOccupees[7, j] = true;
            }
            for (int i = 0; i < 21; i++) casesOccupees[i, 6] = true;
            m1 = 0;
            m2 = 8;
            for (int i = 0; i < 21; i++)
            {
                if (i <= 7 || i >= 12)
                {
                    if (i <= 6)
                    {
                        if (tabNiveauCorrection[m1] == 1) imageQR[i, 8] = new Pixel(0, 0, 0);
                        m1++;
                    }
                    if (i >= 13 && i != 14)
                    {
                        if (tabNiveauCorrection[m2] == 1) imageQR[i, 8] = new Pixel(0, 0, 0);
                        m2++;
                    }
                    casesOccupees[i, 8] = true;
                }
                if (i <= 7 || i >= 13) casesOccupees[i, 7] = true;
                if (i >= 13) casesOccupees[i, 13] = true;
            }
            return casesOccupees;
        }      //Construit les parties constantes d'un QR code de niveau 1 

        /// <summary>
        /// Construit toutes les parties constantes d'un QR code de niveau 2
        /// </summary>
        /// <param name="imageQR">L'image du QRcode initialisé à la bonne taille permettant de noircir les pixels concernés</param>
        /// <returns>Une matrice de booléens indiquant quelles cases de l'image sont constantes et ne seront donc pas modifiées lors de l'écriture du QR code</returns>
        public static bool[,] ConstructionQRcodeNiveau2(Pixel[,] imageQR)
        {
            bool[,] casesOccupees = new bool[25, 25];
            for (int i = 0; i < 25; i++) for (int j = 0; j < 25; j++) casesOccupees[i, j] = false;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)             //construction motif de recherche bas gauche
                {
                    if (i == 0 || i == 6) imageQR[i, j] = new Pixel(0, 0, 0);
                    else if ((j % 2 == 0 || j == 3) && i != 1 && i != 5) imageQR[i, j] = new Pixel(0, 0, 0);
                    if (j == 0 || j == 6) imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int i = 24; i >= 18; i--)              //construction motif de recherche haut gauche
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i == 24 || i == 18) imageQR[i, j] = new Pixel(0, 0, 0);
                    else if ((j % 2 == 0 || j == 3) && i != 23 && i != 19) imageQR[i, j] = new Pixel(0, 0, 0);
                    if (j == 0 || j == 6) imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int i = 24; i >= 18; i--)              //construction motif de recherche haut droite
            {
                for (int j = 24; j >= 18; j--)
                {
                    if (i == 24 || i == 18) imageQR[i, j] = new Pixel(0, 0, 0);
                    else if ((j % 2 == 0 || j == 21) && i != 23 && i != 19) imageQR[i, j] = new Pixel(0, 0, 0);
                    if (j == 24 || j == 18) imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int x = 8; x < 18; x += 2)                     //construction séparateurs
            {
                imageQR[18, x] = new Pixel(0, 0, 0);
                imageQR[x, 6] = new Pixel(0, 0, 0);
                casesOccupees[18, x] = true;
                casesOccupees[18, x + 1] = true;
                casesOccupees[x, 6] = true;
                casesOccupees[x + 1, 6] = true;
            }
            imageQR[7, 8] = new Pixel(0, 0, 0);
            casesOccupees[7, 8] = true;

            int[] tabNiveauCorrection = { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
            int m1 = 0;
            int m2 = 7;
            for (int j = 0; j < 25; j++)
            {
                if (j <= 8 || j >= 17)
                {
                    if (j <= 8 && j != 6)                   //remplissage des bits composant le niveau de correcteur et le type de masque
                    {
                        if (tabNiveauCorrection[m1] == 1) imageQR[16, j] = new Pixel(0, 0, 0);
                        m1++;
                    }
                    if (j >= 17)
                    {
                        if (tabNiveauCorrection[m2] == 1) imageQR[16, j] = new Pixel(0, 0, 0);
                        m2++;
                    }
                    casesOccupees[16, j] = true;
                }
                if (j <= 7 || j >= 17) casesOccupees[17, j] = true;
                if (j <= 7) casesOccupees[7, j] = true;
            }
            for (int i = 0; i < 25; i++) casesOccupees[i, 6] = true;
            m1 = 0;
            m2 = 8;
            for (int i = 0; i < 25; i++)
            {
                if (i <= 7 || i >= 16)
                {
                    if (i <= 6)
                    {
                        if (tabNiveauCorrection[m1] == 1) imageQR[i, 8] = new Pixel(0, 0, 0);
                        m1++;
                    }
                    if (i >= 17 && i != 18)
                    {
                        if (tabNiveauCorrection[m2] == 1) imageQR[i, 8] = new Pixel(0, 0, 0);
                        m2++;
                    }
                    casesOccupees[i, 8] = true;
                }
                if (i <= 7 || i >= 17) casesOccupees[i, 7] = true;
                if (i >= 17) casesOccupees[i, 17] = true;
            }

            for (int i=4; i<9; i++)
            {
                for (int j=16; j<21; j++)
                {
                    imageQR[i, j] = new Pixel(0, 0, 0);
                    casesOccupees[i, j] = true;
                }
            }
            for (int i = 5; i < 8; i++)
            {
                for (int j = 17; j < 20; j++)
                {
                    if (i!=6 || j!=18) imageQR[i, j] = new Pixel(255, 255, 255);
                }
            }

            return casesOccupees;
        }       //Construit les parties constantes d'un QR code de niveau 2 

        /// <summary>
        /// Transforme une chaine de caractères en un tableau de valeurs binaires de 2 char par 2 char sur 11 bits chacune
        /// </summary>
        /// <param name="chaine">La chaine de caractères à convertir</param>
        /// <returns>Un tableau de tableaux comportants les valeurs binaires sur 11 bits de la chaine de caractère en paramètre en prenant 2 caractères par 2 caractères</returns>
        public static int[][] ConvertStringToTabBinaire(string chaine)
        {
            int taille;
            if (chaine.Length % 2 == 0) taille = chaine.Length / 2;
            else taille = chaine.Length / 2 + 1;
            int[][] tabBinaire = new int[taille][];
            char[] lettres1 = new char[1];
            char[] lettres2 = new char[2];
            int k = 0;
            for (int i = 0; i < chaine.Length; i += 2)                 //transformation chaine de caractère en suite de valeurs binaires
            {
                if (i != chaine.Length - 1)
                {
                    lettres2[0] = chaine[i];
                    lettres2[1] = chaine[i + 1];
                    tabBinaire[k] = Convert2CharTo11Bits(lettres2);
                    k++;
                }
                else
                {
                    lettres1[0] = chaine[i];
                    tabBinaire[k] = Convert2CharTo11Bits(lettres1);
                    k++;
                }
            }
            return tabBinaire;
        }           //Transforme une chaine de caractères en valeurs binaires sous forme de tableau de tableau

        /// <summary>
        /// Construit un QR code de niveau 1 générant une chaine de caractère précisée en paramètre
        /// </summary>
        /// <param name="chaine">La chaine de caractère générée par le QR code</param>
        /// <returns>L'image du QR code générant la chaine de caractère -chaine-</returns>
        public static MyImage QRcodeNiveau1(string chaine)
        {
            Pixel[,] imageQR = new Pixel[21, 21];
            MyImage nouvelleImage = null;
            for (int i = 0; i < 21; i++) for (int j = 0; j < 21; j++) imageQR[i, j] = new Pixel(255, 255, 255);
            bool[,] casesOccupees = ConstructionQRcodeNiveau1(imageQR);
            
            int taille;
            if (chaine.Length % 2 == 0) taille = chaine.Length / 2;
            else taille = chaine.Length / 2 + 1;
            int[][] tabBinaire = ConvertStringToTabBinaire(chaine);
            bool legit = true;                                                          //vérifie que les caractères sont compris en alphanumérique
            for (int i = 0; i < taille; i++) if (tabBinaire[i] == null) legit = false;

            if (legit == true)
            {
                int tailleComplet = 17;
                for (int i = 0; i < taille; i++) for (int j = 0; j < tabBinaire[i].Length; j++) tailleComplet++;
                if (tailleComplet > 152) tailleComplet = 152;
                int bourrage = tailleComplet % 8;
                switch (bourrage)                                   //ajout du bon nombre de 0 en fin de conversion en binaire de la chaine de caractère pour avoir un multiple de 8
                {
                    case 1:
                        tailleComplet += 7;
                        break;
                    case 2:
                        tailleComplet += 6;
                        break;
                    case 3:
                        tailleComplet += 5;
                        break;
                    case 4:
                        tailleComplet += 4;
                        break;
                    case 5:
                        tailleComplet += 3;
                        break;
                    case 6:
                        tailleComplet += 2;
                        break;
                    case 7:
                        tailleComplet += 1;
                        break;
                    default:
                        break;
                }

                int[] tabComplet = new int[tailleComplet];                  //début de la suite de binaire par l'indicateur de mode et du nombre de caractères
                tabComplet[0] = 0;
                tabComplet[1] = 0;
                tabComplet[2] = 1;
                tabComplet[3] = 0;
                int[] tabNombreCaracteres = ConvertIntTo9Bits(chaine.Length);
                for (int i = 4; i < 13; i++) tabComplet[i] = tabNombreCaracteres[i - 4];

                int n = 13;
                for (int i = 13; i < tabBinaire.Length + 13; i++)
                {
                    for (int j = 0; j < tabBinaire[i - 13].Length; j++)  //on rentre la suite binaire de la chaine de caractère dans le tableau de byte complet
                    {
                        tabComplet[n] = tabBinaire[i - 13][j];
                        n++;
                    }
                }
                /*if (bourrage == 0) bourrage = 8;
                tabComplet[tailleComplet - 4 - (8 - bourrage)] = 0;             //ajout de la terminaison
                tabComplet[tailleComplet - 3 - (8 - bourrage)] = 0;
                tabComplet[tailleComplet - 2 - (8 - bourrage)] = 0;
                tabComplet[tailleComplet - 1 - (8 - bourrage)] = 0;
                for (int i = tailleComplet - (8 - bourrage); i < tailleComplet; i++) tabComplet[i] = 0;*/

                int compteurCasesOccuppees = 0;
                for (int i = 0; i < 21; i++) for (int j = 0; j < 21; j++) if (casesOccupees[i,j]==true) compteurCasesOccuppees++;           //compte le nombre de cases intouchables
                int nombrePixelsBourrage = 21 * 21 - compteurCasesOccuppees - tailleComplet - 7 * 8;                //compte le nombre de pixels total que l'on doit remplir par la complétion de données

                byte[] nombreByteFromBinaire = new byte[tailleComplet / 8 + nombrePixelsBourrage/8];
                int[][] donneesCompleteBinaire = new int[tailleComplet / 8 + nombrePixelsBourrage/8][];    //prépare le tableau de binaire pour le transformer en tableau d'octets pour la correction
                int[] tabBourrage = { 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 };
                int a = 0;
                int b = 0;
                for (int i=0; i<tailleComplet/8 + nombrePixelsBourrage/8; i++)
                {
                    donneesCompleteBinaire[i] = new int[8];
                    if (i < tailleComplet / 8)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            donneesCompleteBinaire[i][j] = tabComplet[a];
                            a++;
                        }
                    }
                    else
                    {
                        for (int j=0; j<8; j++)
                        {
                            donneesCompleteBinaire[i][j] = tabBourrage[b];
                            if (b < tabBourrage.Length - 1) b++;
                            else b = 0;
                        }
                    }
                }
                for (int i = 0; i < tailleComplet / 8 + nombrePixelsBourrage / 8; i++) nombreByteFromBinaire[i] = Convert_Binary_To_Byte(donneesCompleteBinaire[i]);

                //préparation de la correction
                byte[] correctionBinaire = ReedSolomonAlgorithm.Encode(nombreByteFromBinaire, 7, ErrorCorrectionCodeType.QRCode);
                int[][] tabCorrection = new int[7][];
                int[] tabCorrectionComplet = new int[7 * 8];
                
                int cmptr = 0;
                int compteurCorrection = 0;
                for (int i = 0; i < 7; i++)
                {
                    tabCorrection[i] = Convert_Byte_To_Binary(correctionBinaire[i]);
                    for (int m = 0; m < 8; m++)
                    {
                        tabCorrectionComplet[cmptr] = tabCorrection[i][m];
                        cmptr++;
                    }
                }

                bool[,] casesCorrection = new bool[21, 21];
                compteurCorrection = 0;
                for (int j = 0; j < 21; j ++)
                {
                    for (int i = 0; i < 12 && compteurCorrection < 7 * 8; i++)
                    {
                        if (casesOccupees[i, j] == false)
                        {
                            casesCorrection[i, j] = true;
                            compteurCorrection++;
                        }
                    }
                }

                int compteur = 0;
                int compteurBourrage = 0;
                compteurCorrection = 0;
                for (int j = 20; j >= 3; j -= 4)
                {
                    for (int i = 0; i < 21; i++)     //complétion de la traduction de la chaine en binaire en pixels puis répétition de tabBourrage jusqu'à ce que le QR code soit rempli
                    {
                        
                        if (casesOccupees[i, j] == false && casesCorrection[i, j] == false)
                        {
                            if (compteur < tailleComplet)
                            {
                                if (tabComplet[compteur] == 1) imageQR[i, j] = new Pixel(0, 0, 0);
                                compteur++;
                                if (compteur < tailleComplet && tabComplet[compteur] == 1) imageQR[i, j - 1] = new Pixel(0, 0, 0);
                                compteur++;
                            }
                            else
                            {
                                if (tabBourrage[compteurBourrage] == 1) imageQR[i, j] = new Pixel(0, 0, 0);
                                if (compteurBourrage < 15) compteurBourrage++;
                                else compteurBourrage = 0;
                                if (tabBourrage[compteurBourrage] == 1) imageQR[i, j - 1] = new Pixel(0, 0, 0);
                                if (compteurBourrage < 15) compteurBourrage++;
                                else compteurBourrage = 0;
                            }
                        }
                        if (casesOccupees[i, j] == false && casesCorrection[i, j] == true)
                        {
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j - 1] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                        }
                    }
                    if (j == 8) j--;
                    for (int i = 20; i >= 0; i--)
                    {
                        if (casesOccupees[i, j] == false && casesCorrection[i, j] == false)
                        {
                            if (compteur < tailleComplet)
                            {
                                if (tabComplet[compteur] == 1) imageQR[i, j - 2] = new Pixel(0, 0, 0);
                                compteur++;
                                if (compteur < tailleComplet && tabComplet[compteur] == 1) imageQR[i, j - 3] = new Pixel(0, 0, 0);
                                compteur++;
                            }
                            else
                            {
                                if (tabBourrage[compteurBourrage] == 1) imageQR[i, j - 2] = new Pixel(0, 0, 0);
                                if (compteurBourrage < 15) compteurBourrage++;
                                else compteurBourrage = 0;
                                if (tabBourrage[compteurBourrage] == 1) imageQR[i, j - 3] = new Pixel(0, 0, 0);
                                if (compteurBourrage < 15) compteurBourrage++;
                                else compteurBourrage = 0;
                            }
                        }
                        if (casesOccupees[i, j-2] == false && casesCorrection[i, j-2] == true)
                        {
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j - 2] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j - 3] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                        }
                    }
                }

                for (int i = 0; i < 21; i++)
                {
                    for (int j = 0; j < 21; j++)
                    {
                        if (casesOccupees[i, j] == false && (i + j) % 2 == 0)                   //application du masquage 0
                        {
                            if (imageQR[i, j].R == 0) imageQR[i, j] = new Pixel(255, 255, 255);
                            else imageQR[i, j] = new Pixel(0, 0, 0);
                        }
                    }
                }
                Pixel[,] imageQR2 = AjoutBordsBlancs(imageQR);
                nouvelleImage = new MyImage("BitMap", 25 * 25 * 3 + 54, 25 * 25 * 3, 25, 25, 24, imageQR2);
            }
            else
            {
                for (int i = 0; i < 21; i++) for (int j = 0; j < 21; j++) imageQR[i, j] = new Pixel(255, 255, 255);       //renvoie un carré blanc s'il y a un caractère non lu dans la chaine à convertir
                nouvelleImage = new MyImage("BitMap", 21 * 21 * 3 + 54, 21 * 21 * 3, 21, 21, 24, imageQR);
            }
            return nouvelleImage;
        }       //Construit un QR code de niveau 1 générant la chaine de caractère -chaine-

        /// <summary>
        /// Construit un QR code de niveau 2 générant une chaine de caractère précisée en paramètre
        /// </summary>
        /// <param name="chaine">La chaine de caractère générée par le QR code</param>
        /// <returns>L'image du QR code générant la chaine de caractère -chaine-</returns>
        public static MyImage QRcodeNiveau2(string chaine)
        {
            Pixel[,] imageQR = new Pixel[25, 25];
            MyImage nouvelleImage = null;
            for (int i = 0; i < 25; i++) for (int j = 0; j < 25; j++) imageQR[i, j] = new Pixel(255, 255, 255);
            bool[,] casesOccupees = ConstructionQRcodeNiveau2(imageQR);

            int taille;
            if (chaine.Length % 2 == 0) taille = chaine.Length / 2;
            else taille = chaine.Length / 2 + 1;
            int[][] tabBinaire = ConvertStringToTabBinaire(chaine);
            bool legit = true;
            for (int i = 0; i < taille; i++) if (tabBinaire[i] == null) legit = false;
            if (legit==true)
            {
                int tailleComplet = 17;
                for (int i = 0; i < taille; i++) for (int j = 0; j < tabBinaire[i].Length; j++) tailleComplet++;
                if (tailleComplet > 272) tailleComplet = 272;
                int bourrage = tailleComplet % 8;
                switch (bourrage)                                   //ajout du bon nombre de 0 en fin de conversion en binaire de la chaine de caractère pour avoir un multiple de 8
                {
                    case 1:
                        tailleComplet += 7;
                        break;
                    case 2:
                        tailleComplet += 6;
                        break;
                    case 3:
                        tailleComplet += 5;
                        break;
                    case 4:
                        tailleComplet += 4;
                        break;
                    case 5:
                        tailleComplet += 3;
                        break;
                    case 6:
                        tailleComplet += 2;
                        break;
                    case 7:
                        tailleComplet += 1;
                        break;
                    default:
                        break;
                }

                int[] tabComplet = new int[tailleComplet];                  //début de la suite de binaire par l'indicateur de mode et du nombre de caractères
                tabComplet[0] = 0;
                tabComplet[1] = 0;
                tabComplet[2] = 1;
                tabComplet[3] = 0;
                int[] tabNombreCaracteres = ConvertIntTo9Bits(chaine.Length);
                for (int i = 4; i < 13; i++) tabComplet[i] = tabNombreCaracteres[i - 4];

                int n = 13;
                for (int i = 13; i < tabBinaire.Length + 13; i++)
                {
                    for (int j = 0; j < tabBinaire[i - 13].Length; j++)  //on rentre la suite binaire de la chaine de caractère dans le tableau de byte complet
                    {
                        tabComplet[n] = tabBinaire[i - 13][j];
                        n++;
                    }
                }
                if (bourrage == 0) bourrage = 8;
                tabComplet[tailleComplet - 4 - (8 - bourrage)] = 0;             //ajout de la terminaison
                tabComplet[tailleComplet - 3 - (8 - bourrage)] = 0;
                tabComplet[tailleComplet - 2 - (8 - bourrage)] = 0;
                tabComplet[tailleComplet - 1 - (8 - bourrage)] = 0;
                for (int i = tailleComplet - (8 - bourrage); i < tailleComplet; i++) tabComplet[i] = 0;

                int compteurCasesOccuppees = 0;
                for (int i = 0; i < 25; i++) for (int j = 0; j < 25; j++) if (casesOccupees[i, j] == true) compteurCasesOccuppees++;           //compte le nombre de cases intouchables
                int nombrePixelsBourrage = 25 * 25 - compteurCasesOccuppees - tailleComplet - 10 * 8 - 7;                //compte le nombre de pixels total que l'on doit remplir par la complétion de données

                //prépare le tableau de binaire pour le transformer en tableau d'octets pour la correction
                int[] tabBourrage = { 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 };

                int compteurBinaire = 0;
                int compteurTableauBourrage = 0;
                int[][] donneesCompleteBinaire = new int[34][];
                byte[] nombreByteFromBinaire = new byte[34];
                for (int i = 0; i < 34; i++)
                {
                    donneesCompleteBinaire[i] = new int[8];
                    for (int j = 0; j < 8; j++)
                    {
                        if (compteurBinaire < tailleComplet)
                        {
                            donneesCompleteBinaire[i][j] = tabComplet[compteurBinaire];
                            compteurBinaire++;
                        }
                        else
                        {
                            donneesCompleteBinaire[i][j] = tabBourrage[compteurTableauBourrage];
                            if (compteurTableauBourrage < 15) compteurTableauBourrage++;
                            else compteurTableauBourrage = 0;
                        }
                    }
                }
                int[] tabCompletementEntier = new int[272];
                int a = 0;
                for (int i = 0; i < 34; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        tabCompletementEntier[a] = donneesCompleteBinaire[i][j];
                        a++;
                    }
                }

                for (int i = 0; i < tailleComplet / 8 + nombrePixelsBourrage / 8; i++) nombreByteFromBinaire[i] = Convert_Binary_To_Byte(donneesCompleteBinaire[i]);
                
                //préparation de la correction
                byte[] correctionBinaire = ReedSolomonAlgorithm.Encode(nombreByteFromBinaire, 10, ErrorCorrectionCodeType.QRCode);
                
                int[][] tabCorrection = new int[10][];
                int[] tabCorrectionComplet = new int[10 * 8];
                int cmptr = 0;
                int compteurCorrection = 0;
                for (int i = 0; i < 10; i++)
                {
                    tabCorrection[i] = Convert_Byte_To_Binary(correctionBinaire[i]);
                    for (int m = 0; m < 8; m++)
                    {
                        tabCorrectionComplet[cmptr] = tabCorrection[i][m];
                        cmptr++;
                    }
                }

                int compteurBitsAZero = 0;
                bool[,] casesBitsAZero = new bool[25, 25];
                for (int i=0; i<25 && compteurBitsAZero<7; i++)
                {
                    if (casesOccupees[i, 0] == false)
                    {
                        casesOccupees[i, 0] = true;
                        casesBitsAZero[i, 0] = true;
                        compteurBitsAZero++;
                    }
                    if (casesOccupees[i, 1] == false && compteurBitsAZero<7)
                    {
                        casesOccupees[i, 1] = true;
                        casesBitsAZero[i, 1] = true;
                        compteurBitsAZero++;
                    }
                }

                bool[,] casesCorrection = new bool[25, 25];
                for (int j = 0; j < 25 - 3; j += 4)
                {
                    for (int i = 0; i < 25 && compteurCorrection < 10 * 8; i++)
                    {
                        if (casesOccupees[i, j] == false)
                        {
                            casesCorrection[i, j] = true;
                            compteurCorrection++;
                        }
                        if (casesOccupees[i, j + 1] == false && compteurCorrection < 10 * 8)
                        {
                            casesCorrection[i, j + 1] = true;
                            compteurCorrection++;
                        }
                    }
                    if (j == 4) j++;
                    for (int i = 24; i >= 0 && compteurCorrection < 10 * 8; i--)
                    {
                        if (casesOccupees[i, j + 2] == false)
                        {
                            casesCorrection[i, j + 2] = true;
                            compteurCorrection++;
                        }
                        if (casesOccupees[i, j + 3] == false && compteurCorrection < 10 * 8)
                        {
                            casesCorrection[i, j + 3] = true;
                            compteurCorrection++;
                        }
                    }
                }


                int compteur = 0;
                compteurCorrection = 0;
                for (int j = 24; j >= 3; j -= 4)
                {
                    for (int i = 0; i < 25; i++)     //complétion de la traduction de la chaine en binaire en pixels puis répétition de tabBourrage jusqu'à ce que le QR code soit rempli
                    {
                        if (compteur < 272)
                        {
                            if (casesOccupees[i, j] == false && casesCorrection[i, j] == false)
                            {
                                if (tabCompletementEntier[compteur] == 1) imageQR[i, j] = new Pixel(0, 0, 0);
                                compteur++;
                            }
                            if (casesOccupees[i, j - 1] == false && casesCorrection[i, j - 1] == false)
                            {
                                if (compteur < 272 && tabCompletementEntier[compteur] == 1) imageQR[i, j - 1] = new Pixel(0, 0, 0);
                                compteur++;
                            }
                        }
                        if (casesOccupees[i, j] == false && casesCorrection[i, j] == true)
                        {
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                        }
                        if (casesOccupees[i, j - 1] == false && casesCorrection[i, j - 1] == true)
                        {
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j - 1] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                        }
                    }
                    if (j == 8) j--;
                    for (int i = 24; i >= 0; i--)
                    {
                        if (compteur < 272)
                        {
                            if (casesOccupees[i, j - 2] == false && casesCorrection[i, j - 2] == false)
                            {
                                if (tabCompletementEntier[compteur] == 1) imageQR[i, j - 2] = new Pixel(0, 0, 0);
                                compteur++;
                            }
                            if (casesOccupees[i, j - 3] == false && casesCorrection[i, j - 3] == false)
                            {
                                if (compteur < 272 && tabCompletementEntier[compteur] == 1) imageQR[i, j - 3] = new Pixel(0, 0, 0);
                                compteur++;
                            }
                        }
                        if (casesOccupees[i, j - 2] == false && casesCorrection[i, j - 2] == true)
                        {
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j - 2] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                        }
                        if (casesOccupees[i, j - 3] == false && casesCorrection[i, j - 3] == true)
                        {
                            if (tabCorrectionComplet[compteurCorrection] == 1) imageQR[i, j - 3] = new Pixel(0, 0, 0);
                            compteurCorrection++;
                        }
                    }
                }

                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        if ((casesOccupees[i, j] == false || casesBitsAZero[i, j] == true) && (i + j) % 2 == 0)
                        {
                            if (imageQR[i, j].R == 0) imageQR[i, j] = new Pixel(255, 255, 255);
                            else imageQR[i, j] = new Pixel(0, 0, 0);
                        }
                    }
                }

                Pixel[,] imageQR2 = AjoutBordsBlancs(imageQR);
                nouvelleImage = new MyImage("BitMap", 29 * 29 * 3 + 54, 29 * 29 * 3, 29, 29, 24, imageQR2);
            }
            else
            {
                for (int i = 0; i < 25; i++) for (int j = 0; j < 25; j++) imageQR[i, j] = new Pixel(255, 255, 255);
                nouvelleImage = new MyImage("BitMap", 25 * 25 * 3 + 54, 25 * 25 * 3, 25, 25, 24, imageQR);
            }
            return nouvelleImage;
        }       //Construit un QR code de niveau 2 générant la chaine de caractère -chaine-


        public MyImage LectureQRcode()
        {
            short niveau = 1;
            if (this.largeurImage == 27) niveau = 2;
            switch (niveau)
            {
                case 1:
                    Pixel[,] inutile = new Pixel[21, 21];
                    bool[,] casesOccupees = ConstructionQRcodeNiveau1(inutile);
                    int compteurCasesOccupees = 0;
                    for (int i = 0; i < 21; i++) for (int j = 0; j < 21; j++) if (casesOccupees[i, j] == true) compteurCasesOccupees++;
                    int[][] tabBinaireTotal = new int[(21*21 - compteurCasesOccupees - 7*8) / 8][];
                    for (int i = 0; i < tabBinaireTotal.Length; i++) tabBinaireTotal[i] = new int[8];

                    int a = 0;
                    int b = 0;
                    for (int j=20; j>=3; j-=4)
                    {
                        for (int i=0; i<21; i++)
                        {
                            if (casesOccupees[i, j] == false)
                            {
                                if (this.image[i, j].R == 0) tabBinaireTotal[a][b] = 1;
                                if (b < 7) b++;
                                else
                                {
                                    b = 0;
                                    a++;
                                }
                            }
                        }
                        for (int i=20; i>=0; i--)
                        {
                            if (casesOccupees[i, j] == false)
                            {
                                if (this.image[i, j].R == 0) tabBinaireTotal[a][b] = 1;
                                if (b < 7) b++;
                                else
                                {
                                    b = 0;
                                    a++;
                                }
                            }
                        }
                    }

                    byte[] tabByteTotal = new byte[tabBinaireTotal.Length];
                    for (int i = 0; i < tabByteTotal.Length; i++) tabByteTotal[i] = Convert_Binary_To_Byte(tabBinaireTotal[i]);
                    break;
            }
            return null;
        }
    }
}