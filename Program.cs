using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Minecraft_Server_Inspector
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                // 遍历每个参数并做出相应反应  
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "-watchnew":
                            // 处理 -updatelog 参数  
                            Console.WriteLine("更新日志");
                            Console.WriteLine("-------------------------------------------");
                            Console.WriteLine("1. 修复了程序因上个使用连接未完全释放导致的崩溃或报错。该问题已处理成每个连接完成后直接关闭防止冲突。");
                            Console.WriteLine("2. 修复了在检测最大玩家数量时如果服务器反馈不是一个值导致的崩溃或报错（并连带服务器已上线人数无法显示的 Bug）。该问题已处理成如果不是数值则反馈。");
                            Console.WriteLine("3. 修复了在检测阶段的局部变量错误导致无法检测的问题。");
                            Console.WriteLine("最后一次构建时间：2023/9/17 12:00");
                            Environment.Exit(0);
                            break;
                        case "-update":

                            Console.WriteLine("在线更新方法");
                            Console.WriteLine("-------------------------------------------");
                            Console.WriteLine("打开发布程序，然后等待程序自动搜索新版本更新即可");
                            Console.WriteLine("-------------------------------------------");
                            Console.WriteLine("离线更新方法");
                            Console.WriteLine("在另一个设备上用浏览器进入 http://game.xltv.top:14514/s/vJSD 从网盘中下载新的离线安装程序，并使用 USB 导入到此设备进行替换安装");
                            Environment.Exit(0);
                            break;
                    default:
                            Console.WriteLine($"未知参数：{arg} 。请使用 -updatelog 参数查看更新日志，或者正常启动。");
                            Environment.Exit(0);
                            break;
                    }
                }
            }

            Console.WriteLine("Minecraft Server Inspector - 一款 Minecraft 服务器全自动检查器");
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("使用 -watchnew 参数查看更新日志。");
            Console.WriteLine("使用 -update 参数获取有关更新软件的方法");
            Console.WriteLine("-------------------------------------------");
            Console.Write("请输入服务器地址 - ");
            string serverAddress = Console.ReadLine(); // 替换用户输入服务器地址  
            Console.Write("请输入端口 - ");
            int serverPort = int.Parse(Console.ReadLine()); // 将用户输入的端口转换为整数  
            
            TcpClient tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(serverAddress, serverPort);
                Console.WriteLine("服务器在线，继续！");
               

                

               

                if (tcpClient != null)
                {
                    tcpClient.Close();
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine("服务器离线，原因：" + ex.Message); // 输出具体的异常信息  

                Console.ReadKey(true);
                Environment.Exit(0);
            }
            finally
            {
                tcpClient = null;

                if (tcpClient != null)
                {
                    tcpClient.Close();
                }
                
            }


            Console.Write("输入服务器的 RCON 密码（若无设置则留空，但请确保服务器配置文件已启用 RCON） - ");
            string RCONpasswd = Console.ReadLine();
            Console.Write("输入服务器的 RCON 端口（若无改动请输入 25575 ，这是默认端口：不要留空，仅允许输入数字） - ");
            int RCONPort = int.Parse(Console.ReadLine()); // RCON 用户输入端口

            Console.WriteLine("正在检查 RCON（查询需要一定时间，请耐心等待）");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("安全提示：为了保证服务器安全，请在开启 RCON 功能后更改默认端口并设置强 RCON 密码。不正确设置 RCON 密码可能导致攻击者攻击服务器！");
            Console.WriteLine("警告：此过程会发送大量 RCON 数据包来获取服务器基本信息（未使用任何外部库），可能导致因请求频繁导致拒绝访问。请勿拿此程序滥用服务器资源！");
            Console.WriteLine("-----------------------------------------------------");

            string serverRCONAddress = serverAddress; // 替换为您的服务器地址
            string RCONpassword = RCONpasswd;

            try
            {

                TcpClient tcpClient_RCONPing = new TcpClient();

                // 使用新的 TcpClient 对象连接到目标服务器  
                tcpClient_RCONPing.Connect(serverAddress, RCONPort);

                // 发送 RCON 登录请求  
                Byte[] sendBytes = Encoding.UTF8.GetBytes("{\"identifier\":\"Minecraft\",\"password\":\"" + RCONpassword + "\",\"type\":\"rcon\"}");
                tcpClient_RCONPing.GetStream().Write(sendBytes, 0, sendBytes.Length);

                // 接收 RCON 登录响应  
                Byte[] receiveBytes = new Byte[1024];
                int bytesReceived = tcpClient_RCONPing.GetStream().Read(receiveBytes, 0, receiveBytes.Length);
                string response = Encoding.UTF8.GetString(receiveBytes, 0, bytesReceived);

                // 检查登录是否成功  
                if (response.Contains("{\"success\":true}"))
                {
                    // 发送查询延迟命令  
                    sendBytes = Encoding.UTF8.GetBytes("{\"identifier\":\"Minecraft\",\"type\":\"request\",\"request\":\"ping\"}");
                    tcpClient_RCONPing.GetStream().Write(sendBytes, 0, sendBytes.Length);

                    // 接收查询延迟响应  
                    bytesReceived = tcpClient_RCONPing.GetStream().Read(receiveBytes, 0, receiveBytes.Length);
                    response = Encoding.UTF8.GetString(receiveBytes, 0, bytesReceived);

                    // 解析延迟值  
                    int latency = Int32.Parse(response.Substring(response.IndexOf(":") + 1, response.Length - response.IndexOf(":") - 2));
                    Console.WriteLine("与服务器成功连接的延迟为 " + latency + " 毫秒。");


                    tcpClient_RCONPing = null;

                    if (tcpClient_RCONPing != null) //复查该网络功能是否被完全释放
                    {
                        tcpClient_RCONPing.Close();
                    }

                    
                }




            }
            catch (Exception ex)
            {



                Console.WriteLine("在获取 RCON 数据包时发生异常：" + ex.Message + "，登录失败！请尝试开启 RCON ，或者检查网络状态！");

                Console.ReadKey(true);


                Environment.Exit(0);
            }


            Console.WriteLine("正在获取 Motd （副标题 / 服务器简介）");


            try
            {

                TcpClient tcpClient_GetMotd = new TcpClient();

                tcpClient_GetMotd.Connect(serverAddress, RCONPort);

                // 向服务器发送认证信息  
                byte[] password = Encoding.UTF8.GetBytes(RCONpassword);
                tcpClient_GetMotd.GetStream().Write(password, 0, password.Length);

                // 从服务器接收 MOTD  
                byte[] buffer = new byte[1024];
                int bytesRead = tcpClient_GetMotd.GetStream().Read(buffer, 0, buffer.Length);
                string motd = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("该服务器的 Motd : " + motd);

                tcpClient_GetMotd = null;

                if (tcpClient_GetMotd != null)
                {
                    tcpClient_GetMotd.Close();
                }

                

            }
            catch (Exception ex)
            {
                Console.WriteLine("在获取 Motd 时发生错误: " + ex.Message);
                Console.ReadKey(true);
                Environment.Exit(0);
            }



            try
            {
                TcpClient tcpClient_PluginGet = new TcpClient();

                tcpClient_PluginGet.Connect(serverAddress, RCONPort);

                // 向服务器发送认证信息  
                byte[] password = Encoding.UTF8.GetBytes(RCONpassword);
                tcpClient_PluginGet.GetStream().Write(password, 0, password.Length);

                // 从服务器接收插件列表  
                byte[] buffer = new byte[1024];
                int bytesRead = tcpClient_PluginGet.GetStream().Read(buffer, 0, buffer.Length);
                string plugins = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // 解析插件列表并输出结果  
                string[] pluginArray = plugins.Split(',');
                Console.WriteLine("已安装的插件：");
                foreach (string plugin in pluginArray)
                {
                    Console.WriteLine(plugin.Trim());
                }

                tcpClient_PluginGet = null;

                if (tcpClient_PluginGet != null)
                {
                    tcpClient_PluginGet.Close();
                }

                

            }
            catch (Exception ex)
            {
                Console.WriteLine("在获取插件时发生错误: " + ex.Message);
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            try
            {
                TcpClient tcpClient_Get_join_Max_Player = new TcpClient();

                tcpClient_Get_join_Max_Player.Connect(serverAddress, RCONPort);

                // 向服务器发送认证信息  
                byte[] password = Encoding.UTF8.GetBytes(RCONpassword);
                Task writeTask = Task.Run(() => tcpClient_Get_join_Max_Player.GetStream().WriteAsync(password, 0, password.Length));
                writeTask.Wait();

                // 从服务器接收玩家列表  
                byte[] buffer = new byte[1024];
                Task readTask = Task.Run(() => tcpClient_Get_join_Max_Player.GetStream().ReadAsync(buffer, 0, buffer.Length));
                readTask.Wait();
                string players = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                // 解析玩家列表并输出结果  
                string[] playerArray = players.Split(',');
                int onlinePlayers = playerArray.Length; // 在线玩家数量  

                Console.WriteLine("当前在线玩家数量: " + onlinePlayers);
                int maxPlayers;
                if (int.TryParse(playerArray[0], out maxPlayers))
                {
                    Console.WriteLine("最大玩家数量: " + maxPlayers);
                }
                else
                {
                    Console.WriteLine("无法解析最大玩家数量。这可能是服务器拒绝提供最大玩家数量或一些服务端没有此功能，请向服务器提供商咨询。");
                }

                Console.WriteLine("-------------------------------------------");

                Console.WriteLine("服务器信息获取结束。感谢你的使用。（连接已安全关闭）");

                tcpClient_Get_join_Max_Player = null;

                if (tcpClient_Get_join_Max_Player != null)
                {
                    tcpClient_Get_join_Max_Player.Close();
                }

                

                Console.ReadKey(true);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("在获取玩家数量时发生错误: " + ex.Message);
                Console.ReadKey(true);
                Environment.Exit(0);
            }

           

        }
    }
}
