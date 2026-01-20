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
            Console.WriteLine("[INFO] Digite a frase e ele ira buscar a próxima palavra, espaço em branco é nova palavra.");
            Console.WriteLine("\"Tecle algo para continuar\"");
           _ =  Console.ReadLine();
            //  Loop de Teste Interativo

            Console.Clear();
            Console.CursorVisible = false;

            int linhaTexto = 2;
            int linhaSugestoes = 5;

            string texto = "";
            string palavraAtual = "";
            string ultimaPalavra = "";
            while (true)
            {
                var tecla = Console.ReadKey(intercept: true);

                if (tecla.Key == ConsoleKey.Escape)
                    break;

                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (palavraAtual.Length > 0)
                        palavraAtual = palavraAtual[..^1];
                }
                else if (tecla.Key == ConsoleKey.Spacebar)
                {
                    if (!string.IsNullOrWhiteSpace(palavraAtual))
                    {
                        ultimaPalavra = palavraAtual;
                        texto += palavraAtual + " ";
                        palavraAtual = "";
                    }
                }
                else if (char.IsLetter(tecla.KeyChar))
                {
                    palavraAtual += tecla.KeyChar;
                }

                // Redesenha texto
                Console.SetCursorPosition(0, linhaTexto);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, linhaTexto);
                Console.Write($"> Texto: {texto}{palavraAtual}");

                // Limpa sugestões
                for (int i = 0; i < 5; i++)
                {
                    Console.SetCursorPosition(0, linhaSugestoes + i);
                    Console.Write(new string(' ', Console.WindowWidth));
                }

                if (string.IsNullOrEmpty(ultimaPalavra))
                    continue;

                var sugestoes = preditor.ObterSugestoes(ultimaPalavra, 3);

                // Mostra sugestões fixas
                Console.SetCursorPosition(0, linhaSugestoes);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"Sugestões para '{ultimaPalavra}':");

                int linha = linhaSugestoes + 1;
                foreach (var (palavra, prob) in sugestoes)
                {
                    Console.SetCursorPosition(0, linha++);
                    Console.Write($"- {palavra} ({prob:P1})");
                }

                Console.ResetColor();
            }

            Console.CursorVisible = true;
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

        // escolha aleatória ponderada (mantido para referência)
        public string? PreverProximaPalavra(string palavraAtual)
        {
            var chave = palavraAtual.ToLower().Trim();

            if (!_transicoes.TryGetValue(chave, out var proximasOpcoes))
                return null;

            int totalOcorrencias = proximasOpcoes.Values.Sum();
            int pivoAleatorio = _randomico.Next(totalOcorrencias);
            int acumulado = 0;

            foreach (var parChaveValor in proximasOpcoes)
            {
                acumulado += parChaveValor.Value;
                if (pivoAleatorio < acumulado)
                    return parChaveValor.Key;
            }
            return proximasOpcoes.Keys.First();
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

            // LINQ para ordenar por 'Value' (contagem) decrescente e calcular a probabilidade
            return proximasOpcoes
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxSugestoes)
                .Select(kvp => (kvp.Key, kvp.Value / totalOcorrencias))
                .ToList();
        }
    }
}