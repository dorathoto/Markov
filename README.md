
# Markov

Muito antes das LLM pequenas, aliás bem antes de qualquer LLM, já tínhamos teclados como o SwiftKey que adivinhava a próxima palavra. Como? Com Cadeias de Markov.

Segue um vídeo muito bom para entender o começo disso:

-   https://www.youtube.com/watch?v=SMZFHefftqw
    

Esse também:

-   https://www.youtube.com/watch?v=4xDX20XgmWA
    

Se quer aprofundar na matemática, recomendo o vídeo do melhor professor de cálculo (UNIVESP):

-   https://www.youtube.com/watch?v=G8bBlMfKPC4
    

**OBS:** Tenho 2 Branches:

1.  **Master:** Ótimo para estudos e entender o conceito.
    
2.  **Reativo:** Onde tento fazer algo conforme você vá digitando, ele vai dando a sugestão (a regra é o espaço em branco).



---------------------
Investigando descobri algumas maravilhas, na época dos celulares T9 como o Nokia 3310 o meu código seria impossível de rodar.
EX: Dictionary<string, int> isso aqui duplicaria muitas palavras e teria um custo impensavel, para um celular que teria menos de 16MB de ram, acredito que essa minha lógica mesmo em C precisaria de uns 500mb de ram.

Descobri a solução: Árvores de Prefixos - Trie

Imagine guardar as palavras: "Pato", "Pata", "Patos".

Normal: "Pato" + "Pata" + "Patos" = 14 caracteres.

Trie: P -> A -> T -> O -> (S) -> A

A Trie comprime o prefixo comum "PAT". Para o T9, cada nó da árvore não era uma letra, mas o dígito do teclado (2 a 9).

Ao digitar 2-2-7-2 (C-A-S-A), o algoritmo descia na árvore: Nó 2 -> Nó 2 -> Nó 7 -> Nó 2.

Lá dentro, ele encontrava uma lista compacta de palavras possíveis naquele nó ("CASA", "BARA", "CAPA").
Nisso eu já achei fantástico ai vi que a SwiftKey começou a usar o DAWG
Enquanto a Trie comprime prefixos (início das palavras), o DAWG comprime também os sufixos (finais).

Se "FLOR" e "AMOR" terminam com "OR", na memória eles apontavam para o mesmo nó final físico.

Isso transformava o dicionário numa malha ultra-compacta, reduzindo o tamanho do texto original em até 80-90%.

Enfim quanto menor os recursos maior os malabarismos, vimos isso no DeepSeek R1
