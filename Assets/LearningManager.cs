
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LearningManager : MonoBehaviour {

    public Image GenChart;
    public Text ScoreText;
    public Text GenText;
    public Text TimeText;
    public Text StateText;
    public Text ScaleText;

    public InputField MutationRateInput;
    public InputField MutationChanceInput;
    public Button MutationChanegButt;

    public string FilesLocation;
    public InputField NetworkToLoadFld;
    public string FileToLoad;

    public float ChartScale;

    private enum LearnStates {Idle, Learning, FinishedLearning };

    private const int SIMULATIONS_NUM = 500;
    private const int MAX_TREAD_NUM = 10;

    //private const int GENS_WITHOUT_RECORD_LIMIT = 100;

    private const int _plotResX = 400;
    private const int _plotResY = 200;


    private int[] _iterationsNumSteps = new int[] {};
    private int[] _iterationsNumVals = new int[1] {1};

    internal int CurrentGen = 0;

    private int _testedSimulations = 0;
    private bool _anyThreadsRunning;
    private int _batchNumOfThreads;
    public int GamesPerIteration = 0;

    public TimeSpan LastRunTime;

    public float LastGenMaxScore = float.MinValue;
    public float AllTimesMaxScore = float.MinValue;
    public float LastGenAvgScore = 0;
    public float AllTimesAvgScore = float.MinValue;
    public float AllTimesAvgScoreGen = 0;

    public int LastRecordBrokenGensAgo = 0;
    public int DecrementsWithNoRecord = 0;

    public bool LoadedFromFile = false;


    internal Simulation[] Simulations;
    internal NuralNetworkModel[] Parents;

    internal Thread[] Threads;

    internal bool IsLearning;

    private System.Random _rnd;
    private long _firstGenStartTime;

    BinaryFormatter _bf = new BinaryFormatter();

    private DateTime _startTime;

    private LearnStates _learnState = LearnStates.Idle;

    private List<GenSummary> GenRecord = new List<GenSummary>();
    private int _chartOffset = 0;
    private bool _isTRacking = true;

    public void Init() {
        _rnd = new System.Random();


        Simulations = new Simulation[SIMULATIONS_NUM];
        for (int i = 0; i < SIMULATIONS_NUM; i++) {
            Simulations[i] = new Simulation(_rnd);
        }

        Parents = new NuralNetworkModel[SIMULATIONS_NUM];
        for (int i = 0; i < SIMULATIONS_NUM; i++) {
            Parents[i] = new NuralNetworkModel(_rnd);
        }

        Threads = new Thread[SIMULATIONS_NUM];

        MutationRateInput.text = NuralNetworkModel.MUTATION_SIZE.ToString();

        MutationChanceInput.text = NuralNetworkModel.MUTATION_Rate.ToString();

        PlotGenRecord();
    }

    public void ChangeMutationRate() {
        NuralNetworkModel.MUTATION_SIZE = float.Parse(MutationRateInput.text);

        NuralNetworkModel.MUTATION_Rate = float.Parse(MutationChanceInput.text);
    }

    void Update() {
        
        if (IsLearning && _learnState == LearnStates.Idle) {


            //create gen
            CreateGen();

            //run one gen
            for (int i = 0; i < SIMULATIONS_NUM; i++) {
                Threads[i] = new Thread(Simulations[i].RunOneTime);
            }
            _testedSimulations = 0;

            _anyThreadsRunning = false;
            _batchNumOfThreads = 0;
            _learnState = LearnStates.Learning;
            StateText.text = "Learning";
        }

        if (_learnState == LearnStates.Learning) {
            if (_testedSimulations < SIMULATIONS_NUM || _anyThreadsRunning) {
                if (!_anyThreadsRunning ) {


                    //run simulation batch

#if UNITY_EDITOR
                    


                    for (int i = 0; i < SIMULATIONS_NUM; i++) {
                        Simulations[i].RunOneTime();
                        _testedSimulations++;
                    }

#else
                    _batchNumOfThreads = Mathf.Min(MAX_TREAD_NUM, SIMULATIONS_NUM - _testedSimulations);

                    _testedSimulations += _batchNumOfThreads;

                    for (int i = 0; i < _batchNumOfThreads; i++) {
                        Threads[_testedSimulations - 1 - i].Start();
                    }
                    

                    _anyThreadsRunning = true;

#endif
                } else {
                    _anyThreadsRunning = false;

                    for (int i = 0; i < _batchNumOfThreads; i++) {
                        _anyThreadsRunning |= Threads[_testedSimulations - 1 - i].IsAlive;
                    }
                }


            } else {
                _learnState = LearnStates.FinishedLearning;
                StateText.text = "FinishedLearning";
            }


        } else if (_learnState == LearnStates.FinishedLearning) {
            //conclude gen
            ConcludeGen();

            

            CurrentGen++;
            _learnState = LearnStates.Idle;
            StateText.text = "Idle";
        }


    }

    private void CreateGen() {
        if (CurrentGen == 0) {
            _firstGenStartTime = DateTime.Now.Ticks;
        }

        _startTime = DateTime.Now;

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
            //UNMARK FOR CONSTANT GAME
            for (int i = 0; i < GamesPerIteration; i++) {
                Simulation.Seeds[i] = _rnd.Next();
            }
        }

        //UNMARK FOR RANDOM GAME
        for (int i = 0; i < GamesPerIteration; i++) {
            Simulation.Seeds[i] = _rnd.Next();
        }

        foreach (Simulation s in Simulations) {
            if (CurrentGen > 0) {
                NuralNetworkModel parentModel1 = PickParent(null);
                NuralNetworkModel parentModel2 = PickParent(parentModel1);


                NuralNetworkModel.PrepareNextGen(parentModel1, parentModel2, s.NuralNetwork, _rnd);
            }
            
            if (gamesPIChanged || LoadedFromFile) {
                s.GamesPerIteration = GamesPerIteration;
            }
        }

        LoadedFromFile = false;
    }

    private NuralNetworkModel PickParent(NuralNetworkModel avoid) {
        bool repeat = false;
        do {
            repeat = false;
            float randomp = UnityEngine.Random.value;
            for (int i = 0; i < SIMULATIONS_NUM; i++) {
                randomp -= Simulations[i].Fitness;
                if (randomp <= 0) {
                    if (Parents[i] != avoid) {
                        //print(s.Fitness);
                        return Parents[i];
                    } else {
                        repeat = true;
                        break;
                    }
                }
            }
        } while (repeat);

        return null;
    }

    private void ConcludeGen() {
        

        Array.Sort<Simulation>(Simulations, (a, b) => {
            return (a.OverallScore < b.OverallScore) ? (1) : ((a.OverallScore > b.OverallScore) ? (-1) : (0));
        });

        if (Simulations[0].OverallScore > AllTimesMaxScore) {
            AllTimesMaxScore = Simulations[0].OverallScore;
        }

        LastGenMaxScore = Simulations[0].OverallScore;

        LastGenAvgScore = 0;
        float LastGenFitnessScore = 0;

        foreach (Simulation s in Simulations) {
            LastGenAvgScore += s.OverallScore;
            LastGenFitnessScore += Mathf.Pow(s.OverallScore, 2f);
        }

        LastGenAvgScore /= Simulations.Length;

        for (int i = 0; i < SIMULATIONS_NUM; i++) {
            Simulations[i].Fitness = Mathf.Pow(Simulations[i].OverallScore, 2f) / LastGenFitnessScore;
            NuralNetworkModel.Duplicate(Simulations[i].NuralNetwork, Parents[i]);
            
        }




        LastRecordBrokenGensAgo++;

        if (LastGenAvgScore > AllTimesAvgScore) {
            AllTimesAvgScore = LastGenAvgScore;
            AllTimesAvgScoreGen = CurrentGen;
            //SaveNetworkToFile(true);
            LastRecordBrokenGensAgo = 0;
            DecrementsWithNoRecord = 0;
        }

       
        LastRunTime = DateTime.Now - _startTime;

        

        /*if (LastRecordBrokenGensAgo > GENS_WITHOUT_RECORD_LIMIT) {
            
            LastRecordBrokenGensAgo = 0;
            NuralNetworkModel.Duplicate(RecordSimulation.NuralNetwork, ParentSimulation.NuralNetwork);

            NuralNetworkModel.ReduceMutationRate();

        }*/

        SaveNetworkToFile(false);

        PlotGenRecord();

        

    }

    private void SaveNetworkToFile(bool isRecord) {
        GenSummary summary = new GenSummary();
        //summary.LastModel = ParentSimulation.NuralNetwork;
        //summary.RecordModel = ParentSimulation.NuralNetwork;
        summary.AvgScore = LastGenAvgScore;
        summary.MaxScore = LastGenMaxScore;
        summary.GenNum = CurrentGen;
        summary.GamesPerIteration = GamesPerIteration;
        summary.FirstGenStartTime = _firstGenStartTime;
        summary.LastRecordBrokenGensAgo = LastRecordBrokenGensAgo;
        summary.DecrementsWithNoRecord = DecrementsWithNoRecord;

        string dirPath = String.Format("{0}/{1}", FilesLocation, _firstGenStartTime);

        /*if (!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }

        FileStream file;
        string destination;
        if (isRecord) {
            destination = String.Format("{0}/GEN_{1}_SCORE_{2}", dirPath, CurrentGen, LastGenMaxScore);
        } else {
            destination = String.Format("{0}/latest", dirPath);
        }*/

        /*if (File.Exists(destination)) {
            file = File.OpenWrite(destination);
        } else {
            file = File.Create(destination);
        }

        _bf.Serialize(file, summary);
        file.Close();*/

        if (!isRecord) {
            GenRecord.Add(summary);
        }

        /*destination = String.Format("{0}/record", dirPath);

        if (File.Exists(destination)) {
            file = File.OpenWrite(destination);
        } else {
            file = File.Create(destination);
        }

        _bf.Serialize(file, GenRecord);
        file.Close();*/
    }

    /*public void LoadNetworkFromFile() {
        string destination = String.Format("{0}/{1}/{2}", FilesLocation, NetworkToLoadFld.text, FileToLoad);
        StreamReader reader = new StreamReader(destination);
        GenSummary summary = _bf.Deserialize(reader.BaseStream) as GenSummary;
        reader.Close();

        CurrentGen = summary.GenNum;
        GamesPerIteration = summary.GamesPerIteration;
        NuralNetworkModel.Duplicate(summary.LastModel, ParentSimulation.NuralNetwork);
        NuralNetworkModel.Duplicate(summary.RecordModel, RecordSimulation.NuralNetwork);
        AllTimesMaxScore = summary.MaxScore;
        _firstGenStartTime = summary.FirstGenStartTime;
        LastRecordBrokenGensAgo = summary.LastRecordBrokenGensAgo;
        DecrementsWithNoRecord = summary.DecrementsWithNoRecord;

        destination = String.Format("{0}/{1}/{2}", FilesLocation, NetworkToLoadFld.text, "record");
        reader = new StreamReader(destination);
        GenRecord = _bf.Deserialize(reader.BaseStream) as List<GenSummary>;
        reader.Close();

        LoadedFromFile = true;

        PlotGenRecord();
    }*/

    private void PlotGenRecord() {
        ScoreText.text = LastGenAvgScore + " / " + AllTimesMaxScore;
        GenText.text = "LastGen: " + CurrentGen + " / " + LastRecordBrokenGensAgo + " Small Step: " + NuralNetworkModel.MUTATION_SIZE;
        TimeText.text = "runtime: " + LastRunTime.TotalSeconds + "sec Num of games: " + GamesPerIteration;
        ScaleText.text = "Scale: " + ChartScale + "Offset: " + _chartOffset;

        if (_isTRacking) {
            while (_chartOffset < GenRecord.Count - _plotResX + 1) {
                _chartOffset++;
            }
        }


        for (int i = 0; i < _plotResX; i++) {
            for (int j = 0; j < _plotResY; j++) {
                Color col;
                if (i + _chartOffset > GenRecord.Count) {
                    col = Color.blue;
                } else if (i + _chartOffset == GenRecord.Count) {
                    col = Color.black;
                } else {
                    if (j < GenRecord[i + _chartOffset].AvgScore * ChartScale) {
                        if (i + _chartOffset == AllTimesAvgScoreGen) {
                            col = Color.yellow;
                        } else {
                            col = Color.red;
                        }
                    } else if (j < GenRecord[i + _chartOffset].MaxScore * ChartScale) {
                        
                        col = Color.blue;
                        
                    } else {
                        col = Color.blue;
                    }
                }
                
                //GenChart.sprite.texture.SetPixel((CurrentGen) % _plotResX, i, col);
                //GenChart.sprite.texture.SetPixel((CurrentGen + 1) % _plotResX, i, Color.black);
                GenChart.sprite.texture.SetPixel(i, j, col);

            }
        }
        GenChart.sprite.texture.Apply();


    }

    public void ScaleGraph(bool isUp) {
        if (isUp) {
            ChartScale *= 2;
        } else {
            ChartScale /= 2;
        }

        PlotGenRecord();
    }

    public void SlideGraph(bool isRight) {
        if (isRight) {
            if (_chartOffset < GenRecord.Count - _plotResX + 1) {
                _chartOffset += 10;
                if (_chartOffset >= GenRecord.Count - _plotResX + 1) {
                    _isTRacking = true;
                } 
            }
        } else {
            _chartOffset = Mathf.Max(0, _chartOffset - 10);
            _isTRacking = false;
        }

        PlotGenRecord();
    }

    [Serializable]
    private class GenSummary {
        public NuralNetworkModel RecordModel;
        public NuralNetworkModel LastModel;
        public int GenNum;
        public float AvgScore;
        public float MaxScore;
        public long FirstGenStartTime;
        public int GamesPerIteration;
        public int LastRecordBrokenGensAgo;
        public int DecrementsWithNoRecord;

        
    }

}

