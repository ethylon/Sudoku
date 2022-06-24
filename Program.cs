using Sudoku;
using Google.OrTools.LinearSolver;
using System.Diagnostics;

var solver = Solver.CreateSolver("BOP");    // Solver boolean
var variables = new Dictionary<(int x, int y, int value), Variable>();
var constrainted = new List<(int x, int y)>();

for (int index = 0; index < 9 * 9; index++)
{
    int x = index / 9;
    int y = index % 9;

    // Pour chaque coordonnée x:y, on crée 9 variables qui représentent les 9 valeur possible pour cette cellule
    // La constraint unicity permet de dire que la somme de ces variables = 1 pour n'en retenir qu'une seule

    var unicity = solver.MakeConstraint(1, 1);
    for (int value = 0; value < 9; value++)
    {
        // Variable boolean => 0 ou 1
        var variable = solver.MakeBoolVar($"v{x + 1}_{y + 1}_{value + 1}");
        variables.Add((x + 1, y + 1, value + 1), variable);
        unicity.SetCoefficient(variable, 1);
    }
}

variables
    .GroupBy(x => x.Key.value)
    .ForAll(grouping =>
    {
        // Pour chaque valeur possible de 1 à 9, on doit définir les règles
        // qui imposent que cette valeur ne soit présente qu'une seule fois...

        grouping
            .GroupBy(x => x.Key.x)
            .ForAll(subGrouping =>
            {
                // ... par ligne
                var unicity = solver.MakeConstraint(1, 1);
                subGrouping.ForAll(x => unicity.SetCoefficient(x.Value, 1));
            });

        grouping
            .GroupBy(x => x.Key.y)
            .ForAll(subGrouping =>
            {
                // ... par colonne
                var unicity = solver.MakeConstraint(1, 1);
                subGrouping.ForAll(x => unicity.SetCoefficient(x.Value, 1));
            });

        grouping
            .GroupBy(x => (
                Math.Floor((x.Key.x - 1) / 3d),
                Math.Floor((x.Key.y - 1) / 3d)))
            .ForAll(subGrouping =>
            {
                // ... par bloc de 3x3
                var unicity = solver.MakeConstraint(1, 1);
                subGrouping.ForAll(x => unicity.SetCoefficient(x.Value, 1));
            });
    });


/* CONTRAINTES DE LA PARTIE DE SUDOKU */
/* ---------------------------------- */

CreateConstraint(5, (1, 1), (2, 1));
CreateConstraint(9, (1, 2));
CreateConstraint(30, (1, 3), (1, 4), (2, 2), (2, 3), (3, 2));
CreateConstraint(9, (1, 5), (1, 6));
CreateConstraint(15, (1, 7), (2, 7), (3, 7));
CreateConstraint(17, (1, 8), (2, 8), (1, 9));
CreateConstraint(12, (2, 4), (3, 4), (4, 4));
CreateConstraint(15, (2, 5), (2, 6));
CreateConstraint(9, (2, 9), (3, 9));
CreateConstraint(5, (3, 1), (4, 1));
CreateConstraint(12, (3, 3), (4, 3));
CreateConstraint(5, (3, 5), (3, 6));
CreateConstraint(13, (3, 8), (4, 8));
CreateConstraint(14, (4, 2), (5, 2), (5, 1));
CreateConstraint(9, (4, 5), (5, 5));
CreateConstraint(14, (4, 6), (4, 7));
CreateConstraint(7, (4, 9), (5, 9));
CreateConstraint(15, (5, 3), (5, 4), (6, 3));
CreateConstraint(17, (5, 6), (5, 7));
CreateConstraint(15, (5, 8), (6, 8), (7, 8), (8, 8));
CreateConstraint(15, (6, 1), (7, 1));
CreateConstraint(9, (6, 2), (7, 2));
CreateConstraint(11, (6, 4), (6, 5));
CreateConstraint(5, (6, 6), (7, 6));
CreateConstraint(10, (6, 7), (7, 7));
CreateConstraint(5, (6, 9), (7, 9));
CreateConstraint(12, (7, 3), (8, 3), (7, 4));
CreateConstraint(15, (7, 5), (8, 5), (8, 4));
CreateConstraint(20, (8, 1), (8, 2), (9, 1), (9, 2));
CreateConstraint(11, (8, 6), (8, 7));
CreateConstraint(16, (8, 9), (9, 9));
CreateConstraint(15, (9, 3), (9, 4));
CreateConstraint(4, (9, 5), (9, 6));
CreateConstraint(10, (9, 7), (9, 8));

/* OUTPUT */
/* ------ */

Console.WriteLine(solver.Solve());
Console.WriteLine();
Console.Write("  ");
for (int x = 0; x < 9; x++)
{
    for (int y = 0; y < 9; y++)
    {
        for (int value = 0; value < 9; value++)
        {
            var variable = variables[(x + 1, y + 1, value + 1)];
            if (Math.Round(variable.SolutionValue()) != 0d)
                Console.Write(value + 1 + " ");
        }

        if (y == 2 || y == 5)
            Console.Write("| ");
    }

    Console.Write(Environment.NewLine);
    if (x == 2 || x == 5)
        Console.WriteLine(" -----------------------");
    Console.Write("  ");
}

Console.ReadKey();

/// <summary>
/// Permet d'imposer que la sommes des valeurs présentes dans les cellules
/// `constrainedVariables` soit égale à `objective`
/// </summary>
void CreateConstraint(int objective, params (int x, int y)[] constrainedVariables)
{
    var constraint = solver.MakeConstraint(objective, objective);
    constrainedVariables
        .ForAll(key =>
        {
            Debug.Assert(!constrainted.Contains(key));
            constrainted.Add(key);

            variables
                .Where(x => x.Key.x == key.x && x.Key.y == key.y)
                .ForAll(keyValue =>
                {
                    var variable = keyValue.Value;
                    constraint.SetCoefficient(variable, keyValue.Key.value);
                });
        });
}
