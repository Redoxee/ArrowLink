using System.IO;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

namespace ArrowLink
{
    public class GameSaver
    {
        FlatBufferBuilder m_builder = new FlatBufferBuilder(256);

        public const string c_saveName = "gamesState.sav";

        private static string s_fullName = null;
        public static string SaveFullname
        {
            get
            {
                if(s_fullName == null)
                    s_fullName = Application.persistentDataPath + "/" + c_saveName;
                return s_fullName;
            }
        }


        public int Version = 0;
        public int[] BoardState = new int[16];
        public int CurrentTile = 0;
        public int NextTile = 0;
        public int HoldTile = 0;
        public int BankTarget = 0;
        public int BankState = 0;
        public int CrunchState = 0;
        public int OverLinkState = 0;
        public int Score = 0;
        public int TileScore = 0;
        public int ComboCounter = 0;
        public int CrunchCounter = 0;

        public int DistributorDifficultyLevel = 0;
        public int[] DistributorPrecedence = new int[8];

        public void Save()
        {
            m_builder.Clear();

            var precedenceOffset = Distributor.CreatePrecedenceVector(m_builder, DistributorPrecedence);
            var distributorOffset = Distributor.CreateDistributor(m_builder, DistributorDifficultyLevel, precedenceOffset);
            var boardOffset = GameState.CreateBoardVector(m_builder, BoardState);
            var gameStateOffset = GameState.CreateGameState(m_builder,
                Version,
                boardOffset,
                CurrentTile,
                NextTile,
                HoldTile,
                BankTarget,
                BankState,
                CrunchState,
                OverLinkState,
                Score,
                TileScore,
                ComboCounter,
                CrunchCounter,
                distributorOffset);

            GameState.FinishGameStateBuffer(m_builder, gameStateOffset);

            using (var ms = new MemoryStream(m_builder.DataBuffer.ToFullArray(), m_builder.DataBuffer.Position, m_builder.Offset))
            {
                File.WriteAllBytes(SaveFullname, ms.ToArray());
                Debug.Log("data saved");
            }
        }

        public bool Load()
        {
            string name = SaveFullname;
            if (!File.Exists(name))
                return false;

            ByteBuffer bb = new ByteBuffer(File.ReadAllBytes(name));
            GameState save = GameState.GetRootAsGameState(bb);

            //public int Version = 0;
            for (int i = 0; i < save.BoardLength; ++i)
                BoardState[i] = save.Board(i);

            CurrentTile = save.CurrentTile;
            NextTile = save.NextTile;
            HoldTile = save.HoldTile;
            BankTarget = save.BankTarget;
            BankState = save.BankState;
            CrunchState = save.CrunchState;
            OverLinkState = save.OverLinkState;
            Score = save.Score;
            TileScore = save.TileScore;
            ComboCounter = save.ComboCounter;
            CrunchCounter = save.CrunchCounter;
            var flatDistrib = save.DistributorState.GetValueOrDefault();
            DistributorDifficultyLevel = flatDistrib.DifficultyLevel;
            for (int i = 0; i < flatDistrib.PrecedenceLength; ++i)
                DistributorPrecedence[i] = flatDistrib.Precedence(i); 

            return true;
        }

        public static void DeleteGameSaved()
        {
            string name = SaveFullname;
            if (File.Exists(name))
                File.Delete(name);
        }
    }
}