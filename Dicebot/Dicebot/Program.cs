using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Dicebot
{
  class Program
  {
    private DiscordSocketClient _client;
    public static CommandService _commands;
    public static IServiceProvider _services;

    static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
      _client = new DiscordSocketClient(new DiscordSocketConfig
      {
        LogLevel = LogSeverity.Info
      });
      _client.Log += Log;
      _commands = new CommandService();
      _services = new ServiceCollection().BuildServiceProvider();
      _client.MessageReceived += CommandRecieved;
      //次の行に書かれているstring token = "hoge"に先程取得したDiscordTokenを指定する。
      string token = "";
      await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();

      await Task.Delay(-1);
    }

    /// <summary>
    /// 何かしらのメッセージの受信
    /// </summary>
    /// <param name="msgParam"></param>
    /// <returns></returns>
    private async Task CommandRecieved(SocketMessage messageParam)
    {
      var message = messageParam as SocketUserMessage;

      //デバッグ用メッセージを出力
      Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);
      //メッセージがnullの場合
      if (message == null)
        return;

      //発言者がBotの場合無視する
      if (message.Author.IsBot)
        return;
      var id = message.Author.Id;

      var context = new CommandContext(_client, message);

      //ここから記述--------------------------------------------------------------------------
      var CommandContext = message.Content;
      string Dicecount;//ダイスの面の数
      string DiceNum;//ダイスの面の数
      string NumTimes;//ダイスを振る回数
      string ClearNum;//危険度
      string ClearTimes;//難易度
      int seed = Environment.TickCount;

      int Random_basic(string DiceNum)
      {
        seed++;
        Random rnd = new Random(seed++);
        int m1 = rnd.Next(1, int.Parse(DiceNum) + 1);
        return m1;
      }
      int List_Sum(List<int> list1)
      {
        int num = 0;
        for (int i = 0; i < list1.Count; i++)
        {
          num += list1[i];
        }
        return num;
      }

      // コマンド("!d")かどうか判定
      if (CommandContext.Substring(0, 2) == "!d")
      {

        if (CommandContext.Substring(2) == "")
        {
          string help = $"<@{id}>" + "　:「!d」の使い方説明\n";
          help += "!d3の場合、dの後の3はダイス面になり、3面ダイスを1回ふることになります。\n" +
            "!d3 n4の場合、nの後の4は振る回数になり、3面ダイスを4回ふることになります。\n" +
            "!d3 n4 c1 t1の場合、cの後の数字は成功値、tの後の数字は成功度となります。";
          await message.Channel.SendMessageAsync(help);
        }
        else if (CommandContext.Substring(2) != "")
        {

          if (int.TryParse(CommandContext.Substring(2), out int DiceNum1))
          {
            seed = Environment.TickCount;
            Random rnd = new Random(seed++);
            string m0 = "";
            string m1 = rnd.Next(1, DiceNum1 + 1).ToString();
            m0 += $"<@{id}>" + "　:　" + DiceNum1 + "面ダイス" + "　:　出目は " + m1 + " です。\n";
            await message.Channel.SendMessageAsync(m0);
          }
          //ダイスを振る回数が10回を超える場合
          else if (int.TryParse(CommandContext.Substring(CommandContext.IndexOf("n") + 1), out int NumTimes1) && NumTimes1 > 10)
          {
            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "10回以下でお願いします");
          }
          //ダイスを10回以下振る場合
          else if (int.TryParse(CommandContext.Substring(CommandContext.IndexOf("n") + 1), out NumTimes1))
          {
            //ダイスの面の数
            DiceNum = CommandContext.Substring(2, CommandContext.IndexOf("n") - 2);
            seed = Environment.TickCount;
            string m0 = "";
            m0 += $"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes1 + "回ふります。" + "\n";
            for (int i = 0; i < NumTimes1; i++)
            {
              seed += i;
              Random rnd = new Random(seed++);
              string m1 = rnd.Next(1, int.Parse(DiceNum) + 1).ToString();
              m0 += $"<@{id}>" + "　:　" + (i + 1).ToString() + "　:　出目は " + m1 + " です。\n";
            }
            await message.Channel.SendMessageAsync(m0);
          }
          else if (!(int.Parse(CommandContext.Substring(CommandContext.IndexOf("n") + 1, CommandContext.IndexOf("c") - CommandContext.IndexOf("n") - 1)) < 11))
          {
            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "10回以下でお願いします");
          }
          else
          {
            //ダイスの面の数
            DiceNum = CommandContext.Substring(2, CommandContext.IndexOf("n") - 2);
            //ダイスを振る回数
            NumTimes = CommandContext.Substring(CommandContext.IndexOf("n") + 1, CommandContext.IndexOf("c") - CommandContext.IndexOf("n") - 1);
            //ClearNumは成功値
            ClearNum = CommandContext.Substring(CommandContext.IndexOf("c") + 1, CommandContext.IndexOf("t") - CommandContext.IndexOf("c") - 1);
            //ClearTimesは成功度
            ClearTimes = CommandContext.Substring(CommandContext.IndexOf("t") + 1);

            //難易度がダイス面より大きい場合
            if (!(int.Parse(ClearNum) < int.Parse(DiceNum) + 1))
              return;
            //成功地がダイスを振る回数より大きいとき
            if (!(int.Parse(ClearTimes) < int.Parse(NumTimes) + 1))
              return;

            string m0 = "";
            m0 += $"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "回ふります。" + "成功値" + ClearNum + "の成功度" + ClearTimes + "です。\n";
            int j = 0;
            seed = Environment.TickCount;
            for (int i = 0; i < int.Parse(NumTimes); i++)
            {
              seed += i + j;
              Random rnd = new Random(seed++);
              string m1 = rnd.Next(1, int.Parse(DiceNum) + 1).ToString();
              if (int.Parse(m1) >= int.Parse(ClearNum))
              {
                j++;
                m0 += $"<@{id}>" + "　:　" + (i + 1).ToString() + "　:　" + j.ToString() + "回目の成功：" + m1 + "\n";
                if (j == int.Parse(ClearTimes))
                {
                  m0 += $"<@{id}>" + "　:　" + "成功度" + j.ToString() + "をクリアしたのでダイスを止めます。" + "\n";
                  //j成功値に達するとfor文から脱出
                  if (j == int.Parse(ClearTimes))
                    break;
                }
              }
              else
              {
                m0 += $"<@{id}>" + "　:　" + (i + 1).ToString() + "　:　" + "　　失敗　 ：" + m1 + "\n";
              }
            }
            await message.Channel.SendMessageAsync(m0);
          }
        }
      }

      ////////////////////////////////////////////////////////////////////////////
      if (CommandContext.Substring(0, 2) == "!r")
      {

        if (CommandContext.Substring(2) == "")
        {
          string help = $"<@{id}>" + "　:「!r」の使い方説明\n";
          help += "!r3 d4 の場合、rの後の3はダイスの個数、dの後の4はダイス面、つまり、4面ダイスを3個を1回ふることになります。\n" +
            "!r3 d3 n4 の場合、nの後の4は振る回数になり、3面ダイスを3個を4回ふることになります。\n" +
            "!r3 d3 n4 c1 t1 の場合、cの後の数字は成功値、tの後の数字は成功度となります。";
          await message.Channel.SendMessageAsync(help);
        }
        else if (!(CommandContext[2..] == ""))
        {
          if (int.TryParse(CommandContext[(CommandContext.IndexOf("d") + 1)..], out int DiceNum1))
          {
            DiceNum = CommandContext[(CommandContext.IndexOf("d") + 1)..];
            NumTimes = CommandContext[2..CommandContext.IndexOf("d")];
            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "個振ります");
            List<int> ms = new List<int>();
            for (int i = 0; i < int.Parse(NumTimes); i++)
            {
              ms.Add(Random_basic(DiceNum));
            }
            var s = String.Join(",", ms);
            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　出目は" + s + "で、それらの和は" + List_Sum(ms) + "です");
          }
          else if (int.TryParse(CommandContext[(CommandContext.IndexOf("n") + 1)..], out int NumTimes1) && NumTimes1 > 10)
          {
            await message.Channel.SendMessageAsync($"<@{id}>" + "　:　" + "10回以下でお願いします");
          }
          else if (int.TryParse(CommandContext[(CommandContext.IndexOf("n") + 1)..], out NumTimes1))
          {
            DiceNum = CommandContext.Substring(CommandContext.IndexOf("d") + 1, CommandContext.IndexOf("n") - CommandContext.IndexOf("d") - 1);
            NumTimes = CommandContext[2..CommandContext.IndexOf("d")];
            List<int> ms = new List<int>();
            string m0 = "";
            m0 += $"<@{id}>" + "　:　" + DiceNum + "面ダイスを" + NumTimes + "個を" + NumTimes1 + "回振ります\n";
            seed = Environment.TickCount;
            for (int j = 0; j < NumTimes1; j++)
            {
              for (int i = 0; i < int.Parse(NumTimes); i++)
              {
                seed += i + j;
                Random rnd = new Random(seed);
                int m1 = rnd.Next(1, int.Parse(DiceNum) + 1);
                ms.Add(m1);
              }
              var s = String.Join(" + ", ms);
              m0 += $"<@{id}>" + "　:　" + (j + 1) + "　:　" + s + " = " + List_Sum(ms) + "\n";
              ms.Clear();
            }
            await message.Channel.SendMessageAsync(m0);
          }
          else if (int.TryParse(CommandContext.Substring(CommandContext.IndexOf("c") + 1, CommandContext.IndexOf("t") - CommandContext.IndexOf("c") - 1), out int ClearNum1) && int.TryParse(CommandContext.Substring(CommandContext.IndexOf("t") + 1), out int ClearTimes1))
          {
            //  /r Dicecount  d DiceNum n NumTimes  c ClearNum  t ClearTimes
            //ダイスを振る個数
            Dicecount = CommandContext[2..CommandContext.IndexOf("d")];
            //ダイスの面の数
            DiceNum = CommandContext.Substring(CommandContext.IndexOf("d") + 1, CommandContext.IndexOf("n") - CommandContext.IndexOf("d") - 1);
            //ダイスを振る回数
            NumTimes = CommandContext.Substring(CommandContext.IndexOf("n") + 1, CommandContext.IndexOf("c") - CommandContext.IndexOf("n") - 1);
            //ClearNumは成功値
            ClearNum = CommandContext.Substring(CommandContext.IndexOf("c") + 1, CommandContext.IndexOf("t") - CommandContext.IndexOf("c") - 1);
            //ClearTimesは成功度
            ClearTimes = CommandContext.Substring(CommandContext.IndexOf("t") + 1);

            //難易度がダイス面より大きい場合
            if (!(int.Parse(ClearNum) < (int.Parse(DiceNum) + int.Parse(DiceNum) + 1)))
              return;
            //成功地がダイスを振る回数より大きいとき
            if (!(int.Parse(ClearTimes) < int.Parse(NumTimes) + 1))
              return;

            List<int> ms = new List<int>();
            int k = 0;
            string m0 = "";
            m0 += $"<@{id}>" + "　:　" + DiceNum + "面ダイス" + Dicecount + "個を" + NumTimes + "回ふります。" + "成功値" + ClearNum + "の成功度" + ClearTimes + "です。\n";
            seed = Environment.TickCount;
            for (int j = 0; j < int.Parse(NumTimes); j++)
            {
              for (int i = 0; i < int.Parse(Dicecount); i++)
              {
                seed += i + j;
                Random rnd = new Random(seed);
                int m1 = rnd.Next(1, int.Parse(DiceNum) + 1);
                ms.Add(m1);
              }
              var s = String.Join(" + ", ms);
              if (List_Sum(ms) >= int.Parse(ClearNum))
              {
                k++;
                m0 += $"<@{id}>" + "　:　" + (j + 1) + "　:　" + k + "回目の成功：" + s + " = " + List_Sum(ms) + "\n";
                if (k == int.Parse(ClearTimes))
                {
                  m0 += $"<@{id}>" + "　:　" + "成功度" + k + "をクリアしたのでダイスを止めます。" + "\n";
                  //j成功値に達するとfor文から脱出
                  if (k == int.Parse(ClearTimes))
                    break;
                }
              }
              else
              {
                m0 += $"<@{id}>" + "　:　" + (j + 1) + "　:　" + "　　失敗　 ：" + s + " = " + List_Sum(ms) + "\n";
              }
              ms.Clear();
            }
            await message.Channel.SendMessageAsync(m0);
          }
        }
      }
    }

    private Task Log(LogMessage message)
    {
      Console.WriteLine(message.ToString());
      return Task.CompletedTask;
    }
  }
}