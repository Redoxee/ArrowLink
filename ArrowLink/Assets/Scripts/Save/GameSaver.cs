using System.IO;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

namespace ArrowLink
{
    public class GameSaver
    {
        FlatBufferBuilder m_builder = new FlatBufferBuilder(64);

        public const string c_saveName = "gamesState.sav";

        public string SaveFullname
        {
            get
            {
                return Application.persistentDataPath + "/" + c_saveName;
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
    }
}