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
            Console.WriteLine("Que souhaitez-vous faire ?\n"
                            + "1 : Traiter une image\n"
                            + "2 : Dessiner une fractale\n");
            Console.WriteLine("Tapez le numéro d'une fonction pour la lancer, ou n'importe quoi d'autre pour fermer le programme");
            string reponse = Console.ReadLine();
            if (reponse == "2")
            {
                Console.WriteLine("Veuillez choisir la hauteur de votre fractale :");
                int hauteurFractaleMandelbrot = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Veuillez choisir la largeur de votre fractale :");
                int largeurFractaleMandelbrot = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Veuillez choisir un coefficient de couleur rouge :");
                double coefFractaleMandelBrotR = Convert.ToDouble(Console.ReadLine());
                Console.WriteLine("Veuillez choisir un coefficient de couleur verte :");
                double coefFractaleMandelBrotG = Convert.ToDouble(Console.ReadLine());
                Console.WriteLine("Veuillez choisir un coefficient de couleur bleue :");
                double coefFractaleMandelBrotB = Convert.ToDouble(Console.ReadLine());
                MyImage testFractaleMandelbrot = MyImage.FractaleMandelbrot(hauteurFractaleMandelbrot, largeurFractaleMandelbrot, coefFractaleMandelBrotR, coefFractaleMandelBrotG, coefFractaleMandelBrotB);
                testFractaleMandelbrot.From_Image_To_File("testFractaleMandelbrot.bmp");
                Process.Start("testFractaleMandelbrot.bmp");
            }
            if (reponse == "1")
            {
                Console.WriteLine("Veuillez choisir l'image que vous souhaitez traiter : (coco / renard / tigre / beluga / lena / lac / land / nature / galaxie / desert / foret / paysage / sakura / champs / planete / antilope / Test001 / carreTest)");
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
                    reponse = "";
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
                                         + "18 : Afficher l'image avec une matrice de convolution personnalisée\n"
                                         + "19 : Afficher l'histogramme de l'image\n"
                                         + "20 : Appliquer une rotation à l'image de 90° vers la droite\n"
                                         + "21 : Appliquer une rotation à l'image de 90° vers la gauche\n"
                                         + "22 : Cacher une image dans une autre\n"
                                         + "23 : Appliquer un filtre multicolore sur l'image\n"
                                         + "24 : Appliquer un effet de symétrie sur un axe horizontal\n"
                                         + "25 : Appliquer un effet de symétrie sur un axe vertical\n"
                                         + "26 : Appliquer un effet de symétrie sur un axe vertical et horizontal\n"
                                         + "27 : Donner un changement de couleurs de manière aléatoire à l'image\n"
                                         + "28 : Appliquer un effet de saturation à l'image\n"
                                         + "\n"
                                         + "Sélectionnez la fonction désirée ");
                        Console.WriteLine("Tapez le numéro d'une fonction pour la lancer, ou 'Quitter' pour fermer le programme");
                        reponse = Console.ReadLine();
                        switch (reponse)
                        {
                            case "0":
                                do
                                {
                                    if (nom != "coco.bmp" && nom != "tigre.bmp" && nom != "lena.bmp" && nom != "lac.bmp" && nom != "nature.bmp" && nom != "Test001.bmp" && nom != "carreTest.bmp" && nom != "champs.bmp" && nom != "land.bmp" && nom != "antilope.bmp" && nom != "renard.bmp" && nom != "beluga.bmp" && nom != "galaxie.bmp" && nom != "desert.bmp" && nom != "foret.bmp" && nom != "paysage.bmp" && nom != "sakura.bmp" && nom != "planete.bmp")
                                    {
                                        Console.WriteLine("L'image fournie n'est pas un BitMap ou n'existe pas");
                                    }
                                    Console.WriteLine("Veuillez choisir l'image que vous souhaitez traiter : (coco / renard / tigre / beluga / lena / lac / land / nature / galaxie / desert / foret / paysage / sakura / champs / planete / antilope / Test001 / carreTest)");
                                    nom = Console.ReadLine() + ".bmp";
                                }
                                while (nom != "coco.bmp" && nom != "tigre.bmp" && nom != "lena.bmp" && nom != "lac.bmp" && nom != "nature.bmp" && nom != "Test001.bmp" && nom != "carreTest.bmp" && nom != "champs.bmp" && nom != "land.bmp" && nom != "antilope.bmp" && nom != "renard.bmp" && nom != "beluga.bmp" && nom != "galaxie.bmp" && nom != "desert.bmp" && nom != "foret.bmp" && nom != "paysage.bmp" && nom != "sakura.bmp" && nom != "planete.bmp");
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
                                Console.WriteLine("Veuillez choisir l'image à cacher dans celle sélectionnée (coco / renard / tigre / beluga / lena / lac / land / nature / galaxie / desert / foret / paysage / sakura / champs / planete / antilope / Test001 / carreTest)");
                                string imageACacher = Console.ReadLine() + ".bmp";
                                do
                                {
                                    if (imageACacher != "coco.bmp" && imageACacher != "tigre.bmp" && imageACacher != "lena.bmp" && imageACacher != "lac.bmp" && imageACacher != "nature.bmp" && imageACacher != "Test001.bmp" && imageACacher != "carreTest.bmp" && imageACacher != "champs.bmp" && imageACacher != "land.bmp" && imageACacher != "antilope.bmp" && imageACacher != "renard.bmp" && imageACacher != "beluga.bmp" && imageACacher != "galaxie.bmp" && imageACacher != "desert.bmp" && imageACacher != "foret.bmp" && imageACacher != "paysage.bmp" && imageACacher != "sakura.bmp" && imageACacher != "planete.bmp")
                                    {
                                        Console.WriteLine("L'image fournie n'est pas un BitMap ou n'existe pas");
                                        Console.WriteLine("Veuillez choisir l'image que vous souhaitez traiter : (coco / renard / tigre / beluga / lena / lac / land / nature / galaxie / desert / foret / paysage / sakura / champs / planete / antilope / Test001 / carreTest)");
                                        imageACacher = Console.ReadLine() + ".bmp";
                                    }
                                }
                                while (imageACacher != "coco.bmp" && imageACacher != "tigre.bmp" && imageACacher != "lena.bmp" && imageACacher != "lac.bmp" && imageACacher != "nature.bmp" && imageACacher != "Test001.bmp" && imageACacher != "carreTest.bmp" && imageACacher != "champs.bmp" && imageACacher != "land.bmp" && imageACacher != "antilope.bmp" && imageACacher != "renard.bmp" && imageACacher != "beluga.bmp" && imageACacher != "galaxie.bmp" && imageACacher != "desert.bmp" && imageACacher != "foret.bmp" && imageACacher != "paysage.bmp" && imageACacher != "sakura.bmp" && imageACacher != "planete.bmp");
                                if (imageACacher == nom)
                                {
                                    string fonctionACacher;
                                    do
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Menu :\n"
                                             + "1 : Cacher l'image en noir et blanc\n"
                                             + "2 : Cacher l'image en nuances de gris\n"
                                             + "3 : Cacher l'image en couleurs inversées\n"
                                             + "4 : Cacher l'image avec un effet miroir horizontal\n"
                                             + "5 : Cacher l'image avec un effet miroir vertical\n"
                                             + "6 : Cacher la détection des contours de l'image\n"
                                             + "7 : Cacher l'image floutée\n"
                                             + "8 : Cacher l'image avec un effet psychédélique\n"
                                             + "9 : Cacher l'image avec une augmentation des contrastes\n"
                                             + "10 : Cacher l'image avec un renforcement des bords sur la verticale\n"
                                             + "11 : Cacher l'image avec un renforcement des bords sur l'horizontale\n"
                                             + "12 : Cacher l'image avec un effet de repoussage\n"
                                             + "13 : Cacher l'image avec un filtre de Sobel\n"
                                             + "14 : Cacher l'image avec une matrice de convolution personnalisée\n"
                                             + "15 : Cacher un filtre multicolore sur l'image\n"
                                             + "16 : Cacher un effet de symétrie sur un axe horizontal\n"
                                             + "17 : Cacher un effet de symétrie sur un axe vertical\n"
                                             + "18 : Cacher un effet de symétrie sur un axe vertical et horizontal\n"
                                             + "19 : Cacher un changement de couleurs de manière aléatoire\n"
                                             + "20 : Cacher un effet de saturation à l'image\n"
                                             + "\n");
                                        Console.WriteLine("Veuillez choisir la modification de votre image a cacher dans celle sélectionnée");
                                        fonctionACacher = Console.ReadLine();
                                        switch (fonctionACacher)
                                        {
                                            case "1":
                                                testNoirEtBlanc = test.NoirEtBlanc();
                                                testNoirEtBlanc.From_Image_To_File("testNoirEtBlanc.bmp");
                                                imageACacher = "testNoirEtBlanc.bmp";
                                                break;
                                            case "2":
                                                testNuanceDeGris = test.NuanceDeGris();
                                                testNuanceDeGris.From_Image_To_File("testNuanceDeGris.bmp");
                                                imageACacher = "testNuanceDeGris.bmp";
                                                break;
                                            case "3":
                                                testInverse = test.Inverse();
                                                testInverse.From_Image_To_File("testInverse.bmp");
                                                imageACacher = "testInverse.bmp";
                                                break;
                                            case "4":
                                                testMiroirHorizontal = test.MiroirHorizontal();
                                                testMiroirHorizontal.From_Image_To_File("testMiroirHorizontal.bmp");
                                                imageACacher = "testMiroirHorizontal.bmp";
                                                break;
                                            case "5":
                                                testMiroirVertical = test.MiroirVertical();
                                                testMiroirVertical.From_Image_To_File("testMiroirVertical.bmp");
                                                imageACacher = "testMiroirVertical.bmp";
                                                break;
                                            case "6":
                                                testDetectionContours = test.DetectionContours();
                                                testDetectionContours.From_Image_To_File("testDetectionContours.bmp");
                                                imageACacher = "testDetectionContours.bmp";
                                                break;
                                            case "7":
                                                testFlou = test.Flou();
                                                testFlou.From_Image_To_File("testFlou.bmp");
                                                imageACacher = "testFlou.bmp";
                                                break;
                                            case "8":
                                                testPsychedelique = test.Psychedelique();
                                                testPsychedelique.From_Image_To_File("testPsychedelique.bmp");
                                                imageACacher = "testPsychedelique.bmp";
                                                break;
                                            case "9":
                                                testAugmentationContraste = test.AugmentationContraste();
                                                testAugmentationContraste.From_Image_To_File("testAugmentationContraste.bmp");
                                                imageACacher = "testAugmentationContraste.bmp";
                                                break;
                                            case "10":
                                                testRenforcementBordsVertical = test.RenforcementBordsVertical();
                                                testRenforcementBordsVertical.From_Image_To_File("testRenforcementBordsVertical.bmp");
                                                imageACacher = "testRenforcementBordsVertical.bmp";
                                                break;
                                            case "11":
                                                testRenforcementBordsHorizontal = test.RenforcementBordsHorizontal();
                                                testRenforcementBordsHorizontal.From_Image_To_File("testRenforcementBordsHorizontal.bmp");
                                                imageACacher = "testRenforcementBordsHorizontal.bmp";
                                                break;
                                            case "12":
                                                testRepoussage = test.Repoussage();
                                                testRepoussage.From_Image_To_File("testRepoussage.bmp");
                                                imageACacher = "testRepoussage.bmp";
                                                break;
                                            case "13":
                                                testFiltreSobel = test.FiltreSobel();
                                                testFiltreSobel.From_Image_To_File("testFiltreSobel.bmp");
                                                imageACacher = "testFiltreSobel.bmp";
                                                break;
                                            case "14":
                                                testJSP = test.ConvolutionAleatoire();
                                                testJSP.From_Image_To_File("testConvolutionAleatoire.bmp");
                                                imageACacher = "testConvolutionAleatoire.bmp";
                                                break;
                                            case "15":
                                                MyImage testDegrade2 = test.DegradeMulticolore();
                                                testDegrade2.From_Image_To_File("testDegrade.bmp");
                                                imageACacher = "testDegrade.bmp";
                                                break;
                                            case "16":
                                                MyImage testSymetrieHorizontale2 = test.SymetrieHorizontale();
                                                testSymetrieHorizontale2.From_Image_To_File("testSymetrieHorizontale.bmp");
                                                imageACacher = "testSymetrieHorizontale.bmp";
                                                break;
                                            case "17":
                                                MyImage testSymetrieVerticale2 = test.SymetrieVerticale();
                                                testSymetrieVerticale2.From_Image_To_File("testSymetrieVerticale.bmp");
                                                imageACacher = "testSymetrieVerticale.bmp";
                                                break;
                                            case "18":
                                                MyImage testSymetrieCentrale2 = test.SymetrieCentrale();
                                                testSymetrieCentrale2.From_Image_To_File("testSymetrieCentrale.bmp");
                                                imageACacher = "testSymetrieCentrale.bmp";
                                                break;
                                            case "19":
                                                test.CouleurAléatoire().From_Image_To_File("testCouleurAleatoire.bmp");
                                                imageACacher = "testCouleurAleatoire.bmp";
                                                break;
                                            case "20":
                                                test.Saturage().From_Image_To_File("testSaturage.bmp");
                                                imageACacher = "testSaturage.bmp";
                                                break;
                                            default:
                                                Console.WriteLine();
                                                Console.WriteLine("Cet fonction n'existe pas.");
                                                break;
                                        }
                                        if (fonctionACacher != "1" && fonctionACacher != "2" && fonctionACacher != "3" && fonctionACacher != "4" && fonctionACacher != "5" && fonctionACacher != "6" && fonctionACacher != "7" && fonctionACacher != "8" && fonctionACacher != "9" && fonctionACacher != "10" && fonctionACacher != "11" && fonctionACacher != "12" && fonctionACacher != "13" && fonctionACacher != "14" && fonctionACacher != "15" && fonctionACacher != "16" && fonctionACacher != "17" && fonctionACacher != "18" && fonctionACacher != "19" && fonctionACacher != "20")
                                        {
                                            Console.WriteLine("Cette fonction n'existe pas\nAppuyez sur n'importe quelle touche pour revenir au menu");
                                            Console.ReadKey();
                                        }
                                    } while (fonctionACacher != "1" && fonctionACacher != "2" && fonctionACacher != "3" && fonctionACacher != "4" && fonctionACacher != "5" && fonctionACacher != "6" && fonctionACacher != "7" && fonctionACacher != "8" && fonctionACacher != "9" && fonctionACacher != "10" && fonctionACacher != "11" && fonctionACacher != "12" && fonctionACacher != "13" && fonctionACacher != "14" && fonctionACacher != "15" && fonctionACacher != "16" && fonctionACacher != "17" && fonctionACacher != "18" && fonctionACacher != "19" && fonctionACacher != "20");
                                }
                                MyImage testImageCachee = test.CacherImage(imageACacher);
                                testImageCachee.From_Image_To_File("testImageCachee.bmp");
                                Process.Start("testImageCachee.bmp");
                                Console.WriteLine("Appuyez sur une touche pour retrouver l'image cachée");
                                Console.ReadKey();
                                MyImage testImageRetrouvee = testImageCachee.RetrouverImage();
                                testImageRetrouvee.From_Image_To_File("testImageRetrouvee.bmp");
                                Process.Start("testImageRetrouvee.bmp");
                                break;
                            case "23":
                                MyImage testDegrade = test.DegradeMulticolore();
                                testDegrade.From_Image_To_File("testDegrade.bmp");
                                Process.Start("testDegrade.bmp");
                                break;
                            case "24":
                                MyImage testSymetrieHorizontale = test.SymetrieHorizontale();
                                testSymetrieHorizontale.From_Image_To_File("testSymetrieHorizontale.bmp");
                                Process.Start("testSymetrieHorizontale.bmp");
                                break;
                            case "25":
                                MyImage testSymetrieVerticale = test.SymetrieVerticale();
                                testSymetrieVerticale.From_Image_To_File("testSymetrieVerticale.bmp");
                                Process.Start("testSymetrieVerticale.bmp");
                                break;
                            case "26":
                                MyImage testSymetrieCentrale = test.SymetrieCentrale();
                                testSymetrieCentrale.From_Image_To_File("testSymetrieCentrale.bmp");
                                Process.Start("testSymetrieCentrale.bmp");
                                break;
                            case "27":
                                MyImage testCouleurAleatoire = test.CouleurAléatoire();
                                testCouleurAleatoire.From_Image_To_File("testCouleurAleatoire.bmp");
                                Process.Start("testCouleurAleatoire.bmp");
                                break;
                            case "28":
                                MyImage testSaturage = test.Saturage();
                                testSaturage.From_Image_To_File("testSaturage.bmp");
                                Process.Start("testSaturage.bmp");
                                break;
                            case "29":
                                break;
                            case "quitter":
                            case "Quitter":
                            case "QUITTER":
                            case "q":
                            case "Q":
                                break;
                            default:
                                Console.WriteLine();
                                Console.WriteLine("Cette fonction n'existe pas.");
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
            else
            {
                Console.WriteLine("Fin du programme... Appuyez sur une touche pour fermer la fenêtre.");
                Console.ReadKey();
            }
        }
    }
}

