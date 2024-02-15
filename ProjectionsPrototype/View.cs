using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectionsPrototype
{
    internal class View
    {
        public static string Menu()
        {
            Console.WriteLine("=============================");
            Console.WriteLine("+            MENU           +");
            Console.WriteLine("=============================\n\n");
            Console.WriteLine("1- Manter produto");
            Console.WriteLine("2- Visualizar os eventos");
            Console.WriteLine("0- Sair\n");
            Console.WriteLine("Sua escolha:");

            return Console.ReadLine();
        }public static void Loading()
        {
            Console.WriteLine("=============================");
            Console.WriteLine("+          LOADING...       +");
            Console.WriteLine("=============================\n\n");

        }

        public static string SetQuestion(string title)
        {
            Console.WriteLine("=============================");
            Console.WriteLine($"+        {title}");
            Console.WriteLine("=============================\n\n");
            Console.WriteLine("Insira o que se pede:");


            return Console.ReadLine();

        }
        public static int SetQuantity(string title)
        {
            Console.WriteLine("=============================");
            Console.WriteLine($"+        {title}");
            Console.WriteLine("=============================\n\n");
            Console.WriteLine("Insira a quantidade desejada:");

            int quantity;
            while (!int.TryParse(Console.ReadLine(), out quantity))
            {
                Console.WriteLine("Por favor, insira um número inteiro válido.");
                Console.WriteLine("Insira a quantidade desejada:");
            }

            return quantity;
        }

    }
}
