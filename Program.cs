using System.Numerics;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

static void Main()
{
    string[] livros = new string[100];
    string[] linhas = File.ReadAllLines("livros.txt");
    int quantidade = linhas.Length;
    for (int i = 0; i < quantidade; i++)
    {
        livros[i] = linhas[i];
    }
    for (int i = 0; i < quantidade; i++)// importante não usar o livros.Length, pois o array tem 100 posições, mas pode ser que só tenha 10 preenchidas
    {
        Console.WriteLine(livros[i]);
    }
}


static int CarregarFuncionarios(string[] nomes)
{
    int quantidade = 0;
    using (StreamReader leitor =
           new StreamReader("funcionarios.txt")) 
    {
        while (!leitor.EndOfStream && quantidade < nomes.Length)
        {
            nomes[quantidade] = leitor.ReadLine();
            quantidade++;
        }
    }
    return quantidade;
}
static void Main()
{
    string[] funcionarios = new string[50];
    int total = CarregarFuncionarios(funcionarios);
    Console.WriteLine($"Total: {total}");
}


Ana; 8,0; 7,0
Bruno; 6,0; 9,0
Carla; 9,0; 8,0
using System; 
using System.IO; 
class Program
{
    static void Main()
    {
        string[] nomes = new string[40];
        double[] nota1 = new double[40];
        double[] nota2 = new double[40];
        string[] linhas = File.ReadAllLines("notas.csv");
        for (int i = 0; i < linhas.Length; i++)
        {
            string[] campos = linhas[i].Split(';');
            nomes[i] = campos[0].Trim();
            nota1[i] = double.Parse(campos[1].Trim());
            nota2[i] = double.Parse(campos[2].Trim());
        }
    }
}


static int ContarAprovados(double[] medias, int quantidade)
{
    int aprovados = 0; 
    for (int i = 0; i < quantidade; i++)
    {
        if (medias[i] >= 6.0)
            aprovados++;
    }
    return aprovados;
}
static void Main()
{
    double[] medias = { 7.0, 4.5, 8.0, 6.0, 3.0 };
    int total = ContarAprovados(medias, medias.Length);
    Console.WriteLine(total);
}


questão 5
    B

questão 6
    C

questão 7
    C

questão 8
    D

    9)
    static int CarregarAlunos(string arquivo, string[] nomes,
double[] ni, double[] pi, double[] po)
{
    int quantidade = 0;
    using (StreamReader leitor = new StreamReader(arquivo))
    {
        string linha;
        while (linha = leitor.ReadLine() != null && quantidade < nomes.Length && quantidade < ni.Length && quantidade < pi.Length && quantidade < po.Length);
        {
            string[] campos = linha.Split(';');
            nomes[quantidade] = campos[0].Trim();
            ni[quantidade] = double.Parse(campos[1].Trim());
            pi[quantidade] = double.Parse(campos[2].Trim());
            po[quantidade] = double.Parse(campos[3].Trim());
            quantidade++;
        }
        return quantidade;
    }


10)
    static int GerarRelatorio(string[] nomes, double[] ni,
double[] pi, double[] po, int quantidade)
{
        int aprovados = 0;
        for (int i = 0; i < quantidade; i++)
    {
        double media = (0.2*ni[i] + 0.3*pi[i] + 0.5*po[i]);
            string situacao;
            if (media >= 6.0)
                situacao = "Aprovado";
            else if (media >= 4.0 && media < 6.0)
                situacao = "Em Recuperação";
            else
                situacao = "Reprovado";
            Console.WriteLine($"{nomes[i]}: {media:F2} - {situacao}");
    }
    return aprovados;
}
