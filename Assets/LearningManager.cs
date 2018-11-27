
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LearningManager : MonoBehaviour {

    public Image GenChart;
    public Text ScoreText;
    public Text GenText;

    public string FilesLocation;
    public string FileToLoad;

    private const int SIMULATIONS_NUM = 500;
    private const int MAX_TREAD_NUM = 20;

    private const int _plotResX = 400;
    private const int _plotResY = 200;


    private int[] _iterationsNumSteps = new int[1] { 100 };
    private int[] _iterationsNumVals = new int[2] { 10, 5};

    internal int CurrentGen = 0;

    private int _testedSimulations = 0;

    public int GamesPerIteration = 0;

    public float LastGenMaxScore = float.MinValue;
    public float AllTimesMaxScore = float.MinValue;
    public float LastGenAvgScore = 0;
    public float AllTimesAvgScore = float.MinValue;

    public bool LoadedFromFile = false;


    public Simulation[] Simulations;
    public Simulation ParentSimulation;

    public Thread[] Threads;

    internal bool IsLearning;

    private System.Random _rnd;
    private long _firstGenStartTime;

    BinaryFormatter _bf = new BinaryFormatter();

    public void Init() {
        Simulations = new Simulation[SIMULATIONS_NUM];
        for (int i = 0; i < SIMULATIONS_NUM; i++) {
            Simulations[i] = new Simulation();
        }

        ParentSimulation = new Simulation();

        _rnd = new System.Random();

        Threads = new Thread[SIMULATIONS_NUM];
        

        for (int i = 0; i < GenChart.sprite.texture.width; i++) {
            for (int j = 0; j < GenChart.sprite.texture.height; j++) {
                GenChart.sprite.texture.SetPixel(i, j, Color.blue);
            }
        }
        GenChart.sprite.texture.Apply();
    }

    void Update() {
        if (IsLearning) {

            //create gen
            CreateGen();

            //run one gen
            for (int i = 0; i < SIMULATIONS_NUM; i++) {
                Threads[i] = new Thread(Simulations[i].RunOneTime);
            }
            _testedSimulations = 0;

            bool anyThreadsRunning = false;
            int batchNumOfThreads = 0;

            while (_testedSimulations < SIMULATIONS_NUM || anyThreadsRunning) {
                if (!anyThreadsRunning && _testedSimulations < SIMULATIONS_NUM) {

                    //run simulation batch

                    batchNumOfThreads = Mathf.Min(MAX_TREAD_NUM, SIMULATIONS_NUM - _testedSimulations);

                    _testedSimulations += batchNumOfThreads;

                    for (int i = 0; i < batchNumOfThreads; i++) {
                        Threads[_testedSimulations - 1 - i].Start();
                    }
                }

                anyThreadsRunning = false;
                for (int i = 0; i < batchNumOfThreads; i++) {
                    anyThreadsRunning |= Threads[_testedSimulations - 1 - i].IsAlive;
                }
            }

            //conclude gen
            ConcludeGen();

            CurrentGen++;
        }
    }

    private void CreateGen() {
        if (CurrentGen == 0) {
            _firstGenStartTime = DateTime.Now.Ticks;
        }
        bool gamesPIChanged = false;

        if (CurrentGen == 0) {
            GamesPerIteration = _iterationsNumVals[0];
            gamesPIChanged = true;
        } else {
            for (int i = 0; i < _iterationsNumSteps.Length; i++) {
                if (_iterationsNumSteps[i] == CurrentGen) {
                    GamesPerIteration = _iterationsNumVals[i + 1];
                    gamesPIChanged = true;

                    break;
                }
            }
        }

        if (gamesPIChanged || LoadedFromFile) {
            Simulation.Seeds = new int[GamesPerIteration];
            for (int i = 0; i < GamesPerIteration; i++) {
                Simulation.Seeds[i] = _rnd.Next();
            }
        }

        foreach (Simulation s in Simulations) {
            s.CreateNextGen(ParentSimulation, _rnd);
            s.GamesPerIteration = GamesPerIteration;
            if (gamesPIChanged || LoadedFromFile) {
                s.Scores = new float[GamesPerIteration];
            }
        }

        LoadedFromFile = false;
    }

    private void ConcludeGen() {
        Array.Sort(Simulations, delegate (Simulation a, Simulation b) {
            return b.OverallScore.CompareTo(a.OverallScore);
        });

        LastGenMaxScore = Simulations[0].OverallScore;

        LastGenAvgScore = 0;
        foreach (Simulation s in Simulations) {
            LastGenAvgScore += s.OverallScore;
        }
        LastGenAvgScore /= Simulations.Length;
        if (LastGenAvgScore > AllTimesAvgScore) {
            AllTimesAvgScore = LastGenAvgScore;
            SaveNetworkToFile();
        }
        
        //use best model as parent
        NuralNetworkModel.Duplicate(Simulations[0].NuralNetwork, ParentSimulation.NuralNetwork);

        PlotLastGen();
    }

    private void SaveNetworkToFile() {
        GenSummary summary = new GenSummary();
        summary.BestModel = ParentSimulation.NuralNetwork;
        summary.Score = LastGenMaxScore;
        summary.MaxScore = LastGenMaxScore;
        summary.GenNum = CurrentGen;
        summary.GamesPerIteration = GamesPerIteration;
        summary.FirstGenStartTime = _firstGenStartTime;

        FileStream file;

        string destination = String.Format("{0}{1}_GEN_{2}_SCORE_{3}", FilesLocation, _firstGenStartTime, CurrentGen, LastGenMaxScore);

        if (File.Exists(destination)) {
            file = File.OpenWrite(destination);
        } else {
            file = File.Create(destination);
        }

        _bf.Serialize(file, summary);
        file.Close();
    }

    public void LoadNetworkFromFile() {
        string destination = String.Format("{0}{1}", FilesLocation, FileToLoad);
        StreamReader reader = new StreamReader(destination);
        GenSummary summary = _bf.Deserialize(reader.BaseStream) as GenSummary;
        reader.Close();

        CurrentGen = summary.GenNum;
        GamesPerIteration = summary.GamesPerIteration;
        NuralNetworkModel.Duplicate(summary.BestModel, ParentSimulation.NuralNetwork);
        AllTimesMaxScore = summary.MaxScore;
        _firstGenStartTime = summary.FirstGenStartTime;

        LoadedFromFile = true;

    }

    private void PlotLastGen() {
        ScoreText.text = LastGenAvgScore + " / " + AllTimesMaxScore;
        GenText.text = "completed: " + CurrentGen;

        for (int i = 0; i < _plotResY; i++) {
            Color col;
            if (i < LastGenAvgScore / 1f) {
                col = Color.red;
            } else if (i < LastGenMaxScore / 3f) {
                col = Color.green;
            } else {
                col = Color.blue;
            }
            GenChart.sprite.texture.SetPixel((CurrentGen - 1) % _plotResX, i, col);
            GenChart.sprite.texture.SetPixel((CurrentGen) % _plotResX, i, Color.black);

        }
        GenChart.sprite.texture.Apply();

    }

    [Serializable]
    private class GenSummary {
        public NuralNetworkModel BestModel;
        public int GenNum;
        public float Score;
        public float MaxScore;
        public long FirstGenStartTime;
        public int GamesPerIteration;
    }
}

