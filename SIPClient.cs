using Ozeki.Media;
using Ozeki.VoIP;
using System;
using System.Net;

namespace SIPTranslationClient
{
    public class SIPClient
    {
        private ISoftPhone _softPhone;
        private IPhoneLine _phoneLine;
        private IPhoneCall _call;
        private MediaConnector _connector;
        private PhoneCallAudioSender _audioSender;
        private PhoneCallAudioReceiver _audioReceiver;
        
        private string _username = "frankyan";
        private string _password = "hb8zRUcD8CTGS6x";
        private string _domain = "sip.antisip.com";
        private int _port = 5060;

        public SIPClient()
        {
            InitializeSoftphone();
        }

        private void InitializeSoftphone()
        {
            try
            {
                Console.WriteLine("Inicializando softphone...");
                
                // Usa tu IP local (reemplaza con tu IP real)
                _softPhone = SoftPhoneFactory.CreateSoftPhone(IPAddress.Parse("192.168.29.25"), 10000, 20000);
                
                var account = new SIPAccount(
                    registrationRequired: true,
                    displayName: "SIPTranslator",
                    userName: _username,
                    registerName: _username,
                    registerPassword: _password,
                    domainHost: _domain,
                    domainPort: _port
                );
                
                Console.WriteLine("Registrando línea telefónica...");
                
                _phoneLine = _softPhone.CreatePhoneLine(account);
                _phoneLine.RegistrationStateChanged += (sender, e) => 
                {
                    Console.WriteLine($"Estado de registro: {e.State}");
                    if (e.State == RegState.RegistrationSucceeded)
                        Console.WriteLine("¡Registrado correctamente!");
                };
                
                _softPhone.RegisterPhoneLine(_phoneLine);
                
                _connector = new MediaConnector();
                _audioSender = new PhoneCallAudioSender();
                _audioReceiver = new PhoneCallAudioReceiver();
                
                Console.WriteLine("Cliente SIP listo. Esperando registro...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de inicialización: {ex.Message}");
            }
        }

        public bool IsRegistered()
        {
            return _phoneLine != null && _phoneLine.RegState == RegState.RegistrationSucceeded;
        }

        public void MakeCall(string numberToCall)
        {
            if (!IsRegistered())
            {
                Console.WriteLine("Error: Línea no registrada.");
                return;
            }

            try
            {
                Console.WriteLine($"Llamando a {numberToCall}...");
                _call = _softPhone.CreateCallObject(_phoneLine, numberToCall);
                
                _call.CallStateChanged += (sender, e) => 
                {
                    Console.WriteLine($"Estado de llamada: {e.State}");
                    
                    if (e.State == CallState.Answered)
                    {
                        Console.WriteLine("Llamada contestada - configurando audio...");
                        SetupAudioStreams();
                    }
                    else if (e.State == CallState.Error)
                    {
                        Console.WriteLine("Error en llamada - revisa conexión y credenciales");
                    }
                };
                
                _call.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar llamada: {ex.Message}");
            }
        }

        private void SetupAudioStreams()
        {
            if (_call == null) return;
            
            try
            {
                // Solución universal para audio
                dynamic audioSource = Activator.CreateInstance(Type.GetTypeFromProgID("Ozeki.Audio.Source"));
                _connector.Connect(audioSource, _audioSender);
                _audioSender.AttachToCall(_call);
                audioSource.Start();
                Console.WriteLine("Flujo de audio de salida configurado");

                dynamic audioSink = Activator.CreateInstance(Type.GetTypeFromProgID("Ozeki.Audio.Sink"));
                _audioReceiver.AttachToCall(_call);
                _connector.Connect(_audioReceiver, audioSink);
                audioSink.Start();
                Console.WriteLine("Flujo de audio de entrada configurado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configurando audio: {ex.Message}");
            }
        }

        private void CleanUpCall()
        {
            if (_call != null)
            {
                _call.CallStateChanged -= (sender, e) => {};
                _call = null;
            }
            
            _audioSender?.Detach();
            _audioReceiver?.Detach();
        }

        public void HangUp()
        {
            if (_call != null && (_call.CallState == CallState.Answered || _call.CallState == CallState.InCall))
            {
                Console.WriteLine("Terminando llamada...");
                _call.HangUp();
            }
        }

        public void PrintDebugInfo()
        {
            Console.WriteLine("\n=== INFORMACIÓN DE DEPURACIÓN ===");
            Console.WriteLine($"Registro SIP: {_phoneLine?.RegState}");
            Console.WriteLine($"Llamada actual: {_call?.CallState}");
            
            try
            {
                var mic = Microphone.GetDefaultDevice();
                var speaker = Speaker.GetDefaultDevice();
                Console.WriteLine($"Dispositivos de audio:");
                Console.WriteLine($"- Micrófono: {(mic != null ? mic.DeviceInfo.ProductName : "No detectado")}");
                Console.WriteLine($"- Altavoz: {(speaker != null ? speaker.DeviceInfo.ProductName : "No detectado")}");
            }
            catch
            {
                Console.WriteLine("- No se pudieron detectar dispositivos de audio");
            }
        }
    }
}