using Markov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GeradorTextoMarkov
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Inicializando Motor Markoviano (Estilo T9) ---");

            //  Dataset
            var baseDeDados = DataSetFrases.ObterFrases;

            //  Instanciação e Treinamento
            var preditor = new PreditorCadeiaMarkov();
            Console.WriteLine($"[INFO] Treinando com {baseDeDados.Length} sentenças...");

            preditor.Treinar(baseDeDados);

            Console.WriteLine("[INFO] Modelo treinado.");
            Console.WriteLine("[INFO] Digite uma palavra para ver as 3 sugestões mais prováveis.");
            Console.WriteLine("-------------------------------------------------------------");

            //  Loop de Teste Interativo
            while (true)
            {
                Console.Write("\n> Entrada: ");
                var entrada = Console.ReadLine()?.Trim();

                if (string.Equals(entrada, "sair", StringComparison.OrdinalIgnoreCase))
                    break;

                if (string.IsNullOrWhiteSpace(entrada))
                    continue;

                // Obtém as TOP 3 (Estilo T9)
                var sugestoes = preditor.ObterSugestoes(entrada, 3);

                if (sugestoes.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  Sugestões para '{entrada}':");

                    int rank = 1;
                    foreach (var (palavra, probabilidade) in sugestoes)
                    {
                        // Formata como porcentagem para visualização do "Score"
                        Console.WriteLine($"  {rank}. {palavra} ({probabilidade:P1})");
                        rank++;
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"  [!] Nenhuma transição conhecida a partir de '{entrada}'.");
                    Console.ResetColor();
                }
            }
        }
    }

    /// <summary>
    /// Implementação de Cadeia de Markov de 1ª Ordem (Bigram).
    /// </summary>
    public class PreditorCadeiaMarkov
    {
        // Estrutura: Estado Atual -> { Próximos Estados Possíveis : Contagem }
        private readonly Dictionary<string, Dictionary<string, int>> _transicoes = new();
        private readonly Random _randomico = new();

        public void Treinar(IEnumerable<string> frases)
        {
            foreach (var frase in frases)
            {
                // Sanitização básica
                var fraseLimpa = Regex.Replace(frase.ToLower(), @"[^\w\s]", "");
                var tokens = fraseLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < tokens.Length - 1; i++)
                {
                    var atual = tokens[i];
                    var proxima = tokens[i + 1];

                    if (!_transicoes.ContainsKey(atual))
                    {
                        _transicoes[atual] = new Dictionary<string, int>();
                    }

                    if (!_transicoes[atual].ContainsKey(proxima))
                    {
                        _transicoes[atual][proxima] = 0;
                    }

                    _transicoes[atual][proxima]++;
                }
            }
        }



        /// <summary>
        /// Retorna as N palavras mais prováveis ordenadas por probabilidade (Estilo T9).
        /// </summary>
        public List<(string Palavra, double Probabilidade)> ObterSugestoes(string palavraAtual, int maxSugestoes = 3)
        {
            var chave = palavraAtual.ToLower().Trim();

            if (!_transicoes.TryGetValue(chave, out var proximasOpcoes))
            {
                return new List<(string, double)>();
            }

            // Calcula o total para obter a porcentagem
            double totalOcorrencias = proximasOpcoes.Values.Sum();

            return proximasOpcoes
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxSugestoes)
                .Select(kvp => (kvp.Key, kvp.Value / totalOcorrencias))
                .ToList();
        }
    }
}