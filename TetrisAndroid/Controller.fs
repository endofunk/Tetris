//
// Controller.fs
//
// Author:  endofunk
//
// Copyright (c) 2019 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Endofunk
open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open TetrisShared

type Touch =
  | Left
  | Right
  | Rotate
  | Drop
  | Reset

type Controller () as this = 
  inherit Game()

  let graphics = new GraphicsDeviceManager(this)
  let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
  let mutable block = Unchecked.defaultof<Texture2D>
  let mutable empty = Unchecked.defaultof<Texture2D>
  let mutable logo = Unchecked.defaultof<Texture2D>
  let mutable lastKey = Unchecked.defaultof<TimeSpan>
  let mutable lastFall = Unchecked.defaultof<TimeSpan>
  let mutable lastTouch = Unchecked.defaultof<TimeSpan>
  let mutable lastTouchAction = Touch.Reset
  let size = new Point(10, 20)
  let mutable board = [ for _ in 1..size.Y do yield [ for _ in 1..size.X do yield ' ' ] ]         
  let mutable mBoard = board
  let mutable tetromino = Seq.take 1 Tetrominos |> Seq.item 0
  let mutable nextTetromino = Seq.take 1 Tetrominos |> Seq.item 0
  let mutable tPos = new Point(4, 0)
  let mutable dropDuration = 950
  let mutable f16 = Unchecked.defaultof<SpriteFont>
  let mutable f20 = Unchecked.defaultof<SpriteFont>
  let mutable f22 = Unchecked.defaultof<SpriteFont>
  let mutable lines = 0
  let mutable level = 0
  let mutable score = 0
  let mutable gameState = GameState.Launching
  let mutable start = true
  let speedPerLevel = 55

  do
    this.Content.RootDirectory <- "Content"
    this.IsMouseVisible <- true
    graphics.PreferredBackBufferWidth <- 480
    graphics.PreferredBackBufferHeight <- 640

  override this.Initialize() =
    base.Initialize()

  override this.LoadContent() =
    spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    // TODO: use this.LoadContent to load your game content here
    block <- this.Content.Load<Texture2D>("Block")
    empty <- this.Content.Load<Texture2D>("Empty")
    logo <- this.Content.Load<Texture2D>("Logo")
    f16 <- this.Content.Load<SpriteFont>("Font16")
    f20 <- this.Content.Load<SpriteFont>("Font20")
    f22 <- this.Content.Load<SpriteFont>("Font22")
    dropDuration <- 950 - (level * speedPerLevel)

  override this.Update (gameTime) =
    if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) then this.Exit()  

    let leftRect = new Rectangle(0, 0, 160, 480)
    let rightRect = new Rectangle(320, 0, 160, 480)
    let dropRect = new Rectangle(0, 480, 480, 160)
    let rotateRect = new Rectangle(160, 0, 160, 480)

    // TODO: use this.UpdateContent to load your game content here
    if gameState = GameState.Launching && start then
      lastFall <- gameTime.TotalGameTime
      lastTouch <- gameTime.TotalGameTime
      start <- false
    elif gameState = GameState.Launching && (gameTime.TotalGameTime - lastFall).Seconds > 6 then
      lastFall <- gameTime.TotalGameTime
      gameState <- GameState.InGame
    elif gameState = GameState.Launching then
      ()
    else

      if (gameTime.TotalGameTime - lastFall).Milliseconds >= dropDuration then
        lastFall <- gameTime.TotalGameTime
        tPos.Y <- tPos.Y + 1 

      if (gameTime.TotalGameTime - lastTouch).Milliseconds > 100 then
        lastTouch <- gameTime.TotalGameTime
        let touchCollection = TouchPanel.GetState()
        if touchCollection.Count > 0 then
          let touch = touchCollection.Item(0)
          if leftRect.Contains(touch.Position) && lastTouchAction <> Touch.Left then 
            tPos.X <- if (willLeftCollide board tetromino tPos) then tPos.X else tPos.X - 1
            lastTouchAction <- Touch.Left
          elif rightRect.Contains(touch.Position) && lastTouchAction <> Touch.Right then 
            tPos.X <- if (willRightCollide board tetromino tPos) then tPos.X else tPos.X + 1
            lastTouchAction <- Touch.Right
          elif dropRect.Contains(touch.Position) && lastTouchAction <> Touch.Drop then 
            dropDuration <- 5
            lastTouchAction <- Touch.Drop
          elif rotateRect.Contains(touch.Position) && lastTouchAction <> Touch.Rotate then
            tetromino <- rotateLeft tetromino
            lastTouchAction <- Touch.Rotate
            if (xLen tetromino + tPos.X) > xLen board 
            then tPos.X <- tPos.X + (xLen board - (xLen tetromino + tPos.X)) 
            else tPos.X <- tPos.X
          elif lastTouchAction = Touch.Drop then ()
          else lastTouchAction <- Touch.Reset
        
      if (gameTime.TotalGameTime - lastKey).Milliseconds > 100 then
        lastKey <- gameTime.TotalGameTime
        let keyboard = Keyboard.GetState()
        if keyboard.IsKeyDown(Keys.Up) then 
          tetromino <- rotateLeft tetromino
          if (xLen tetromino + tPos.X) > xLen board 
          then tPos.X <- tPos.X + (xLen board - (xLen tetromino + tPos.X)) 
          else tPos.X <- tPos.X
        if keyboard.IsKeyDown(Keys.Left) then tPos.X <- if (willLeftCollide board tetromino tPos) then tPos.X else tPos.X - 1 
        if keyboard.IsKeyDown(Keys.Right) then tPos.X <- if (willRightCollide board tetromino tPos) then tPos.X else tPos.X + 1 
        if keyboard.IsKeyDown(Keys.Down) then dropDuration <- 5

      if (willVerticalCollide board tetromino tPos) || tPos.Y >= (List.length board - List.length tetromino) then
        board <- merge tetromino tPos (fun a -> a <> ' ') board
        mBoard <- board
        tetromino <- nextTetromino
        nextTetromino <- Seq.take 1 Tetrominos |> Seq.item 0
        tPos <- new Point(4, 0)
        dropDuration <- 950 - (level * speedPerLevel)
        if isGameOver mBoard tetromino tPos then
          board <- [ for _ in 1..size.Y do yield [ for _ in 1..size.X do yield ' ' ] ]
          mBoard <- board
          lines <- 0
          level <- 0
          score <- 0
          dropDuration <- 950
      else
        board <- removeLines board 
        let linesRemoved = size.Y - List.length board
        lines <- lines + linesRemoved
        if linesRemoved > 0 then
          score <- score + calcScore level linesRemoved
        board <- board |> addLines size
        mBoard <- merge tetromino tPos (fun a -> a <> ' ') board
        level <- lines / 10

    base.Update(gameTime)

  override this.Draw (gameTime) =
    this.GraphicsDevice.Clear Color.Black

    // TODO: Add your drawing code here
    spriteBatch.Begin()
    if gameState = GameState.Launching then
      spriteBatch.Draw(logo, new Rectangle(0, (this.Window.ClientBounds.Height - logo.Height) / 2, logo.Width, logo.Height), Color.White * 0.7f)
    else
      for y in yRange nextTetromino do
        for x in xRange nextTetromino do
          let c = nextTetromino.[y].[x]
          let p = new Point(x * block.Width + 346, y * block.Height + 70)
          if c <> ' ' then 
            spriteBatch.Draw(block, new Rectangle(p.X, p.Y, block.Width, block.Height), (char2Color c) * 0.7f)
          else spriteBatch.Draw(empty, new Rectangle(p.X, p.Y, empty.Width, empty.Height), Color.Black * 0.1f)

      for y in 0..(size.Y - 1) do
        for x in 0..(size.X - 1) do
          let c = mBoard.[y].[x]
          let p = new Point(x * block.Width + 10, y * block.Height + 20)
          if c <> ' ' then 
            spriteBatch.Draw(block, new Rectangle(p.X, p.Y, block.Width, block.Height), (char2Color c) * 0.7f)
          else spriteBatch.Draw(empty, new Rectangle(p.X, p.Y, empty.Width, empty.Height), Color.GhostWhite * 0.1f)

      spriteBatch.DrawString(f20, "NEXT", new Vector2(355.0f, 20.0f), Color.Yellow * 0.7f)
      spriteBatch.DrawString(f16, "SCORE:", new Vector2(320.0f, 207.0f), Color.Yellow * 0.7f)
      spriteBatch.DrawString(f16, sprintf "%08d" score, new Vector2(320.0f, 235.0f), Color.White * 0.7f)
      spriteBatch.DrawString(f16, "LEVEL", new Vector2(320.0f, 281.0f), Color.Yellow * 0.7f)
      spriteBatch.DrawString(f22, sprintf "%02d" level, new Vector2(405.0f, 276.0f), Color.White * 0.7f)
      spriteBatch.DrawString(f16, "LINES", new Vector2(320.0f, 322.0f), Color.Yellow * 0.7f)
      spriteBatch.DrawString(f22, sprintf "%03d" lines, new Vector2(405.0f, 317.0f), Color.White * 0.7f)
    spriteBatch.End()
    base.Draw(gameTime)