#if NET
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class LoggingTest : IntegrationTestBase
    {
        private readonly SftpClient _sftpClient;

        public LoggingTest()
        {
            _sftpClient = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
        }

        [TestMethod]
        public async Task SessionLoggerFactoryTest()
        {
            // remember the original logger factory and restore it later
            ILoggerFactory originalTestLoggerFactory = SshNetLoggingConfiguration.LoggerFactory;

            try
            {
                FakeLogCollector globalLogCollector = new();
                FakeLogCollector sessionLogCollector = new();

                ILoggerFactory globalLoggerFactory = CreateFakeLoggerFactory(globalLogCollector);
                SshNetLoggingConfiguration.InitializeLogging(globalLoggerFactory);

                _sftpClient.ConnectionInfo.LoggerFactory = CreateFakeLoggerFactory(sessionLogCollector);

                await _sftpClient.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

                // all logs should have been written to the session logger only
                Assert.AreEqual(0, globalLogCollector.Count);
                Assert.IsTrue(sessionLogCollector.Count > 0);
            }
            finally
            {
                SshNetLoggingConfiguration.InitializeLogging(originalTestLoggerFactory);
            }
        }


        private ILoggerFactory CreateFakeLoggerFactory(FakeLogCollector fakeLogCollector)
        {
            FakeLoggerProvider fakeLogProvider = new(fakeLogCollector);

            return LoggerFactory.Create(builder =>
            {
                builder.AddProvider(fakeLogProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sftpClient.Disconnect();
            _sftpClient.Dispose();
        }
    }
}
#endif
