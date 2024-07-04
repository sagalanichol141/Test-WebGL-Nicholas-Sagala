using System;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.Plugins.GoogleCloud.SpeechRecognition.Examples
{
    public class SpeechRecognitionCase01 : MonoBehaviour
    {
        private GCSpeechRecognition _speechRecognition;

        private Image _speechRecognitionState;

        private Button _startRecordButton,
                       _stopRecordButton;

        private InputField _commandsInputField;

        private Text _resultText;

        private Dropdown _languageDropdown;

        public Text finalText;
        public Image imageResult;
        public AudioSource audioResult;
        public Sprite[] imageSprites;
        public AudioClip[] audioClips;

        private void Start()
        {
            _speechRecognition = GCSpeechRecognition.Instance;
            _speechRecognition.RecognizeSuccessEvent += RecognizeSuccessEventHandler;
            _speechRecognition.RecognizeFailedEvent += RecognizeFailedEventHandler;

            _speechRecognition.FinishedRecordEvent += FinishedRecordEventHandler;
            _speechRecognition.StartedRecordEvent += StartedRecordEventHandler;
            _speechRecognition.RecordFailedEvent += RecordFailedEventHandler;

            _speechRecognition.EndTalkigEvent += EndTalkigEventHandler;

            _startRecordButton = transform.Find("Canvas/Button_StartRecord").GetComponent<Button>();
            _stopRecordButton = transform.Find("Canvas/Button_StopRecord").GetComponent<Button>();

            _speechRecognitionState = transform.Find("Canvas/Image_RecordState").GetComponent<Image>();

            _resultText = transform.Find("Canvas/Text_Result").GetComponent<Text>();

            _commandsInputField = transform.Find("Canvas/InputField_Commands").GetComponent<InputField>();

            _languageDropdown = transform.Find("Canvas/Dropdown_Language").GetComponent<Dropdown>();

            _startRecordButton.onClick.AddListener(StartRecordButtonOnClickHandler);
            _stopRecordButton.onClick.AddListener(StopRecordButtonOnClickHandler);

            _startRecordButton.interactable = true;
            _stopRecordButton.interactable = false;
            _speechRecognitionState.color = Color.yellow;

            _languageDropdown.ClearOptions();

            _speechRecognition.RequestMicrophonePermission(null);

            for (int i = 0; i < Enum.GetNames(typeof(Enumerators.LanguageCode)).Length; i++)
            {
                _languageDropdown.options.Add(new Dropdown.OptionData(((Enumerators.LanguageCode)i).Parse()));
            }

            _languageDropdown.value = _languageDropdown.options.IndexOf(_languageDropdown.options.Find(x => x.text == Enumerators.LanguageCode.en_GB.Parse()));


            // select first microphone device
            if (_speechRecognition.HasConnectedMicrophoneDevices())
            {
                _speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[0]);
            }
        }

        private void OnDestroy()
        {
            _speechRecognition.RecognizeSuccessEvent -= RecognizeSuccessEventHandler;
            _speechRecognition.RecognizeFailedEvent -= RecognizeFailedEventHandler;

            _speechRecognition.FinishedRecordEvent -= FinishedRecordEventHandler;
            _speechRecognition.StartedRecordEvent -= StartedRecordEventHandler;
            _speechRecognition.RecordFailedEvent -= RecordFailedEventHandler;

            _speechRecognition.EndTalkigEvent -= EndTalkigEventHandler;
        }

        private void StartRecordButtonOnClickHandler()
        {
            _startRecordButton.interactable = false;
            _stopRecordButton.interactable = true;
            _resultText.text = string.Empty;

            _speechRecognition.StartRecord(false);
        }

        private void StopRecordButtonOnClickHandler()
        {
            _stopRecordButton.interactable = false;
            _startRecordButton.interactable = true;

            _speechRecognition.StopRecord();
        }

        private void StartedRecordEventHandler()
        {
            _speechRecognitionState.color = Color.red;
        }

        private void RecordFailedEventHandler()
        {
            _speechRecognitionState.color = Color.yellow;
            _resultText.text = "<color=red>Start record Failed. Please check microphone device and try again.</color>";

            _stopRecordButton.interactable = false;
            _startRecordButton.interactable = true;
        }

        private void EndTalkigEventHandler(AudioClip clip, float[] raw)
        {
            FinishedRecordEventHandler(clip, raw);
        }

        private void FinishedRecordEventHandler(AudioClip clip, float[] raw)
        {
            if (_startRecordButton.interactable)
            {
                _speechRecognitionState.color = Color.yellow;
            }

            if (clip == null)
                return;

            RecognitionConfig config = RecognitionConfig.GetDefault();
            config.languageCode = ((Enumerators.LanguageCode)_languageDropdown.value).Parse();
            config.audioChannelCount = clip.channels;
            // configure other parameters of the config if need

            GeneralRecognitionRequest recognitionRequest = new GeneralRecognitionRequest()
            {
                audio = new RecognitionAudioContent()
                {
                    content = raw.ToBase64()
                },
                //audio = new RecognitionAudioUri() // for Google Cloud Storage object
                //{
                //	uri = "gs://bucketName/object_name"
                //},
                config = config
            };

            _speechRecognition.Recognize(recognitionRequest);
        }

        private void RecognizeFailedEventHandler(string error)
        {
            _resultText.text = "Recognize Failed: " + error;
        }

        private void RecognizeSuccessEventHandler(RecognitionResponse recognitionResponse)
        {
            _resultText.text = "Detected: ";

            string[] commands = _commandsInputField.text.Split(',');

            foreach (var result in recognitionResponse.results)
            {
                finalText.text = result.alternatives[0].transcript;

                foreach (var alternative in result.alternatives)
                {
                    _resultText.text += "\nIncome text: " + alternative.transcript;

                    foreach (var command in commands)
                    {
                        if (command.Trim(' ').Trim('.').ToLowerInvariant() == alternative.transcript.Trim(' ').Trim('.')
                            .ToLowerInvariant())
                        {
                            _resultText.text += "\nDid command: " + command + ";"; // debug result command

                            DoCommand(command.ToLowerInvariant().TrimEnd(' ').TrimStart(' '));
                        }
                    }
                }
            }
        }

        private void DoCommand(string command)
        {
            if (command == "buka gambar" || command == "show picture")
                imageResult.sprite = imageSprites[UnityEngine.Random.Range(0, imageSprites.Length)];
            if (command == "mainkan suara" || command == "play sound")
            {
                audioResult.clip = audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
                audioResult.Play();
            }
        }
    }
}