namespace Application.Services.Implementations;

public class PromptCreator
{
    public virtual string StartGame(string characterName, string city, string shortLanguageName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterName);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortLanguageName);
        var language = GetLanguage(shortLanguageName);
        var prompt = $"Сгенерируй текст приветствия в текстовом квесте. Мое имя {characterName}.\n\n" +
                     $"**Пример:** Добро пожаловать, {characterName}." +
                     $"Ты прибыл в древний город {city}, где судьба тебе готовит новые испытания на каждом шагу. " +
                     $"Решение, которое ты примешь сегодня, изменит ход истории этого места. Готов ли ты принять вызов?\n\n" +
                     $"Сгенерируй подобный текст на {language} языке.";
        return prompt;
    }

    public virtual string GameSystemContent()
    {
        var message = "Ты мастер игры Подземелье и Драконы.\n\n" +
                      "**Контекст игры:** Средневековое фэнтези. Игрок оказывается в небольшом городе, окруженном лесами и горами. " +
                      "Город находится на пороге важных событий, и игроку предстоит выбрать, каким путем ему пойти - стать героем, " +
                      "который спасет город, или же погрузиться в тьму и разрушить его. " +
                      "Ты должен придумывать для игрока разные ситуации, в которых он может себя проявить\n\n" +
                      "**Описание персонажа:** Игрок - молодой авантюрист, который только что прибыл в город. " +
                      "У него нет четких планов, и он открыт для приключений.\n\n" +
                      "**Тон и стиль:** Текст должен быть приветственным, но загадочным и немного напряженным, " +
                      "чтобы сразу захватить внимание игрока и намекнуть на грядущие испытания. Будь креативен в своих ответах";
        return message;
    }

    public virtual string ImageSystemContent()
    {
        var message = "Ты художник, который рисует в средневековом стиле";
        return message;
    }

    public virtual string GenerateImage(string imageDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageDescription);
        var prompt =
            $"Нарисуй: \"{imageDescription}\"";
        return prompt;
    }

    public virtual string GetThreeAnswers(string textOrAnswer, string shortLanguageName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(textOrAnswer);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortLanguageName);
        var language = GetLanguage(shortLanguageName);
        var prompt =
            $"Есть текст: \"{textOrAnswer}\". Сгенерируй 3 варианта ответа на этот текст " +
            $"или 3 варианта логического продолжения этого текста от первого лица. " +
            $"Представь, что ты игрок игры Подземелья и Драконы. " +
            $"Ответы должны быть разными: первый - положительный, второй - нейтральный, третий - отрицательный. " +
            $"Ответ должен быть на {language} языке. " +
            $"Формат ответа:\n {{\"answers\": {ResponseFormat(shortLanguageName)}}}";
        return prompt;
    }

    public virtual string GenerateLocations(string shortLanguageName)
    {
        var language = GetLanguage(shortLanguageName);
        var prompt =
            $"Придумай 3 названия для города в средневековом фэнтези мире. " +
            $"Ответ должен быть на {language} языке  в виде JSON. " +
            $"Формат ответа:\n{{\"answers\": {ResponseFormat(shortLanguageName)}}}";
        return prompt;
    }

    private string ResponseFormat(string shortLanguageName)
    {
        if (shortLanguageName.Contains("en"))
            return "[\"Answer1\",\"Answer2\",\"Answer3\"]";
        if (shortLanguageName.Contains("ru"))
            return "[\"Ответ1\",\"Ответ2\",\"Ответ3\"]";
        throw new InvalidOperationException($"Unsupported language: {shortLanguageName}");
    }

    private string GetLanguage(string shortLanguageName)
    {
        if (shortLanguageName.Contains("en"))
            return "английском";
        if (shortLanguageName.Contains("ru"))
            return "русском";
        throw new InvalidOperationException($"Unsupported language: {shortLanguageName}");
    }

    public virtual string AcceptChoice(string requestChoice, string shortLanguageName)
    {
        var language = GetLanguage(shortLanguageName);
        var prompt =
            $"Мой ответ - {requestChoice} Придумай игровую ситуацию, которая следует из моего ответа. " +
            $"Ответ должен быть на {language} языке";
        return prompt;
    }
}