using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Media;
using System.ComponentModel;
using System.IO;

namespace PSI___Louis___Meric
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowHeight = 50;
            Console.WindowWidth = 200;
            Console.WriteLine("Veuillez choisir l'image que vous souhaitez traiter : (coco / tigre / lena / lac / nature / Test001 / carreTest)");
            string nom = Console.ReadLine() + ".bmp";
            MyImage test;
            try
            {
                test = new MyImage(nom);
            }
            catch (FileNotFoundException)
            {
                test = new MyImage("pas un bitmap", 0, 0, 0, 0, 0, null);
            }

            if (test.TypeImage == "BitMap")
            {
                string reponse = "";
                do
                {
                    Console.Clear();
                    Console.WriteLine("image choisie : " + nom);
                    Console.WriteLine("type d'image : " + test.TypeImage);
                    Console.WriteLine("taille du fichier : " + test.TailleFichier);
                    Console.WriteLine("taille offset : " + test.TailleOffset);
                    Console.WriteLine("hauteur de l'image : " + test.HauteurImage);
                    Console.WriteLine("largeur de l'image : " + test.LargeurImage);
                    Console.WriteLine("nombre de bits couleur : " + test.NbBitsCouleur);
                    Console.WriteLine();
                    Console.WriteLine("Menu :\n"
                                     + "0 : Changer d'image à traiter\n"
                                     + "1 : Afficher l'image originelle\n"
                                     + "2 : Afficher l'image en noir et blanc\n"
                                     + "3 : Afficher l'image en nuances de gris\n"
                                     + "4 : Afficher l'image en couleurs inversées\n"
                                     + "5 : Afficher l'image avec un effet miroir horizontal\n"
                                     + "6 : Afficher l'image avec un effet miroir vertical\n"
                                     + "7 : Agrandir l'image\n"
                                     + "8 : Réduire l'image\n"
                                     + "9 : Appliquer une rotation à l'image\n"
                                     + "10 : Afficher la détection des contours de l'image\n"
                                     + "11 : Afficher l'image floutée\n"
                                     + "12 : Afficher l'image avec un effet psychédélique\n"
                                     + "13 : Afficher l'image avec une augmentation des contrastes\n"
                                     + "14 : Afficher l'image avec un renforcement des bords sur la verticale\n"
                                     + "15 : Afficher l'image avec un renforcement des bords sur l'horizontale\n"
                                     + "16 : Afficher l'image avec un effet de repoussage\n"
                                     + "17 : Afficher l'image avec un filtre de Sobel\n"
                                     + "18 : Afficher l'image avec une matrice de convolution aléatoire\n"
                                     + "19 : Afficher l'histogramme de l'image\n"
                                     + "20 : Appliquer une rotation à l'image de 90° vers la droite\n"
                                     + "21 : Appliquer une rotation à l'image de 90° vers la gauche\n"
                                     + "\n"
                                     + "Sélectionnez la fonction désirée ");
                    Console.WriteLine("Tapez le numéro d'une fonction pour la lancer, ou 'Quitter' pour fermer le programme");
                    reponse = Console.ReadLine();
                    switch (reponse)
                    {
                        case "0":
                            do
                            {
                                if (nom != "coco.bmp" && nom != "tigre.bmp" && nom != "lena.bmp" && nom != "lac.bmp" && nom != "nature.bmp" && nom != "Test001.bmp" && nom != "carreTest.bmp")
                                {
                                    Console.WriteLine("L'image fournie n'est pas un BitMap ou n'existe pas");
                                }
                                Console.WriteLine("Veuillez choisir l'image que vous souhaitez traiter : (coco / tigre / lena / lac / nature / Test001 / carreTest)");
                                nom = Console.ReadLine() + ".bmp";
                            } 
                            while (nom != "coco.bmp" && nom != "tigre.bmp" && nom != "lena.bmp" && nom != "lac.bmp" && nom != "nature.bmp" && nom != "Test001.bmp" && nom != "carreTest.bmp");
                            test = new MyImage(nom);
                            break;
                        case "1":
                            test.From_Image_To_File("verif_image.bmp");
                            Process.Start("verif_image.bmp");
                            break;
                        case "2":
                            MyImage testNoirEtBlanc = test.NoirEtBlanc();
                            testNoirEtBlanc.From_Image_To_File("testNoirEtBlanc.bmp");
                            Process.Start("testNoirEtBlanc.bmp");
                            break;
                        case "3":
                            MyImage testNuanceDeGris = test.NuanceDeGris();
                            testNuanceDeGris.From_Image_To_File("testNuanceDeGris.bmp");
                            Process.Start("testNuanceDeGris.bmp");
                            break;
                        case "4":
                            MyImage testInverse = test.Inverse();
                            testInverse.From_Image_To_File("testInverse.bmp");
                            Process.Start("testInverse.bmp");
                            break;
                        case "5":
                            MyImage testMiroirHorizontal = test.MiroirHorizontal();
                            testMiroirHorizontal.From_Image_To_File("testMiroir.bmp");
                            Process.Start("testMiroir.bmp");
                            break;
                        case "6":
                            MyImage testMiroirVertical = test.MiroirVertical();
                            testMiroirVertical.From_Image_To_File("testMiroir2.bmp");
                            Process.Start("testMiroir2.bmp");
                            break;
                        case "7":
                            Console.WriteLine("Veuillez choisir un coefficient d'agrandissement (hauteur) :");
                            int coefHauteurAgrandi = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Veuillez choisir un coefficient d'agrandissement (largeur) :");
                            int coefLargeurAgrandi = Convert.ToInt32(Console.ReadLine());
                            MyImage testAgrandi = test.Agrandir(coefHauteurAgrandi, coefLargeurAgrandi);
                            testAgrandi.From_Image_To_File("testAgrandi.bmp");
                            Process.Start("testAgrandi.bmp");
                            break;
                        case "8":
                            Console.WriteLine("Veuillez choisir un coefficient de réduction (hauteur) :");
                            int coefHauteurReduit = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Veuillez choisir un coefficient de réduction (largeur) :");
                            int coefLargeurReduit = Convert.ToInt32(Console.ReadLine());
                            MyImage testReduit = test.Reduire(coefHauteurReduit, coefLargeurReduit);
                            testReduit.From_Image_To_File("testReduit.bmp");
                            Process.Start("testReduit.bmp");
                            break;
                        case "9":
                            Console.WriteLine("Veuillez choisir un angle de rotation vers la droite :");        
                            int angle = Convert.ToInt32(Console.ReadLine());
                            MyImage testRotation = test.Rotation(angle);
                            testRotation.From_Image_To_File("testRotation.bmp");
                            Process.Start("testRotation.bmp");
                            break;
                        case "10":
                            MyImage testDetectionContours = test.DetectionContours();
                            testDetectionContours.From_Image_To_File("testDetectionContours.bmp");
                            Process.Start("testDetectionContours.bmp");
                            break;
                        case "11":
                            MyImage testFlou = test.Flou();
                            testFlou.From_Image_To_File("testFlou.bmp");
                            Process.Start("testFlou.bmp");
                            break;
                        case "12":
                            MyImage testPsychedelique = test.Psychedelique();
                            testPsychedelique.From_Image_To_File("testPsychedelique.bmp");
                            Process.Start("testPsychedelique.bmp");
                            break;
                        case "13":
                            MyImage testAugmentationContraste = test.AugmentationContraste();
                            testAugmentationContraste.From_Image_To_File("testAugmentationContraste.bmp");
                            Process.Start("testAugmentationContraste.bmp");
                            break;
                        case "14":
                            MyImage testRenforcementBordsVertical = test.RenforcementBordsVertical();
                            testRenforcementBordsVertical.From_Image_To_File("testRenforcementBords.bmp");
                            Process.Start("testRenforcementBords.bmp");
                            break;
                        case "15":
                            MyImage testRenforcementBordsHorizontal = test.RenforcementBordsHorizontal();
                            testRenforcementBordsHorizontal.From_Image_To_File("testRenforcementBords2.bmp");
                            Process.Start("testRenforcementBords2.bmp");
                            break;
                        case "16":
                            MyImage testRepoussage = test.Repoussage();
                            testRepoussage.From_Image_To_File("testRepoussage.bmp");
                            Process.Start("testRepoussage.bmp");
                            break;
                        case "17":
                            MyImage testFiltreSobel = test.FiltreSobel();
                            testFiltreSobel.From_Image_To_File("testFiltreSobel.bmp");
                            Process.Start("testFiltreSobel.bmp");
                            break;
                        case "18":
                            MyImage testJSP = test.ConvolutionAleatoire();
                            testJSP.From_Image_To_File("testConvolutionAleatoire.bmp");
                            Process.Start("testConvolutionAleatoire.bmp");
                            break;
                        case "19":
                            MyImage testHistogramme = test.Histogramme();
                            testHistogramme.From_Image_To_File("testHistogramme.bmp");
                            Process.Start("testHistogramme.bmp");
                            break;
                        case "20":
                            MyImage testRotation90Droite = test.Rotation90Droite();
                            testRotation90Droite.From_Image_To_File("testRotation90Droite.bmp");
                            Process.Start("testRotation90Droite.bmp");
                            break;
                        case "21":
                            MyImage testRotation90Gauche = test.Rotation90Gauche();
                            testRotation90Gauche.From_Image_To_File("testRotation90Gauche.bmp");
                            Process.Start("testRotation90Gauche.bmp");
                            break;
                        case "22":
                            Console.WriteLine("Veuillez choisir l'image à cacher dans celle sélectionnée (coco / tigre / lena / lac / nature / Test001 / carreTest)");
                            string imageACacher = Console.ReadLine() + ".bmp";
                            do
                            {
                                if (imageACacher != "coco.bmp" && imageACacher != "tigre.bmp" && imageACacher != "lena.bmp" && imageACacher != "lac.bmp" && imageACacher != "nature.bmp" && imageACacher != "Test001.bmp" && imageACacher != "carreTest.bmp")
                                {
                                    Console.WriteLine("L'image fournie n'est pas un BitMap ou n'existe pas");
                                    Console.WriteLine("Veuillez choisir l'image que vous souhaitez traiter : (coco / tigre / lena / lac / nature / Test001 / carreTest)");
                                    imageACacher = Console.ReadLine() + ".bmp";
                                }
                            }
                            while (imageACacher != "coco.bmp" && imageACacher != "tigre.bmp" && imageACacher != "lena.bmp" && imageACacher != "lac.bmp" && imageACacher != "nature.bmp" && imageACacher != "Test001.bmp" && imageACacher != "carreTest.bmp");
                            MyImage testImageCachee = test.CacherImage(imageACacher);
                            testImageCachee.From_Image_To_File("testImageCachee.bmp");
                            Process.Start("testImageCachee.bmp");
                            break;
                        case "quitter":
                        case "Quitter":
                        case "QUITTER":
                        case "q":
                        case "Q":
                            break;
                        default:
                            Console.WriteLine();
                            Console.WriteLine("Cet fonction n'existe pas.");
                            break;
                    }
                    if (reponse != "Quitter" && reponse != "quitter" && reponse != "QUITTER" && reponse != "q" && reponse != "Q")
                    {
                        Console.WriteLine("Appuyez sur n'importe quelle touche pour revenir au menu");
                        Console.ReadKey();
                    }
                } while (reponse != "Quitter" && reponse != "quitter" && reponse != "QUITTER" && reponse != "q" && reponse != "Q");

                /*Console.WriteLine("Header :");
                for (int i = 0; i < 14; i++) Console.Write(test.FichierComplet[i] + " ");
                Console.WriteLine("\n\nHeader Info :");
                for (int i = 14; i < 54; i++) Console.Write(test.FichierComplet[i] + " ");
                Console.WriteLine("\n\nImage :");
                for (int i = 54; i < test.FichierComplet.Length; i += test.LargeurImage*3)
                {
                    for (int j = i; j < i + test.LargeurImage*3; j++)
                        Console.Write(test.FichierComplet[j] + " ");
                    Console.WriteLine();
                }*/
            }
            else
            {
                Console.WriteLine("L'image fournie n'est pas un BitMap ou n'existe pas");
                Console.WriteLine("Appuyez sur une touche pour fermer le programme :");
                Console.ReadKey();
            }
        }
    }
}

