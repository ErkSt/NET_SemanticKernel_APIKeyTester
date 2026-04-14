using System.ComponentModel;
using Microsoft.SemanticKernel;

public class TextUtilsPlugin
{
    [KernelFunction]
    [Description("Cuenta la cantidad de palabras en un texto.")]
    public int CountWords([Description("El texto a analizar")] string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}