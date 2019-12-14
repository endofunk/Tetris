//
// Tetris.fs
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
namespace TetrisShared
open Microsoft.Xna.Framework

type public GameState =
  | Launching
  | InGame

[<AutoOpen>]
module public Tetris =
  let public rotateLeft (tetromino : 'a list list) = xRange tetromino |> List.map (fun n -> tetromino |> List.map (fun xs -> xs.[n])) |> List.rev
  let public rotateRight (tetromino : 'a list list) = xRange tetromino |> List.map (fun n -> tetromino |> List.rev |> List.map (fun xs -> xs.[n]))

  let public merge (tetromino : 'a list list) (p : Point) f (board : 'a list list) =
    let rec iterX xs r c =
      match xs with
      | [] -> []
      | ch :: cf -> (if c >= p.X && c <= (p.X + (xLen tetromino)) && (f tetromino.[r - p.Y].[c - p.X]) then tetromino.[r - p.Y].[c - p.X] else ch) :: iterX cf r (c + 1)
    let rec iterY xss r =
      match xss with
      | [] -> []
      | rh :: rf -> (if r >= p.Y && r <= (p.Y + (yLen tetromino)) then iterX rh r 0 else rh) :: iterY rf (r + 1)
    iterY board 0

  let private isVerticalCollider (xss : char list list) (p : Point) =
    if xss.[p.Y].[p.X] <> ' ' && (p.Y = (yLen xss) || xss.[p.Y + 1].[p.X] = ' ') then [p] 
    else []

  let private isHorizontalCollider (xss : char list list) (p : Point) =
    if xss.[p.Y].[p.X] <> ' ' && (p.X = (xLen xss) || p.X = 0 || xss.[p.Y].[p.X + 1] = ' ' || xss.[p.Y].[p.X - 1] = ' ') then [p] 
    else []

  let private getColliders xss f = yRange xss |> List.collect (fun y -> xRange xss |> List.collect (fun x -> f xss (new Point(x, y))))
  let private getVerticalColliders tetromino = getColliders tetromino isVerticalCollider 
  let private getHorizontalColliders tetromino = getColliders tetromino isHorizontalCollider 
  
  let private checkVerticalCollision (xss : char list list) (p : Point) = 
    if p.Y = (yLen xss) || xss.[p.Y + 1].[p.X] <> ' ' then true 
    else false

  let private checkLeftCollision (board : char list list) (p : Point) = 
    if p.X = 0 then true
    elif board.[p.Y].[p.X - 1] <> ' ' then true 
    else false

  let private checkRightCollision (board : char list list) (p : Point) = 
    if p.X = xLen board then true
    elif board.[p.Y].[p.X + 1] <> ' ' then true 
    else false

  let private willCollide (board : char list list) (tetromino : char list list) (p : Point) f g = 
    tetromino |> f |> List.map (fun a -> a + p) |> List.filter (g board) |> List.length > 0

  let public willVerticalCollide board tetromino position = willCollide board tetromino position getVerticalColliders checkVerticalCollision
  let public willLeftCollide board tetromino position = willCollide board tetromino position getHorizontalColliders checkLeftCollision
  let public willRightCollide board tetromino position = willCollide board tetromino position getHorizontalColliders checkRightCollision
    
  let public isGameOver board tetromino position = 
    if willVerticalCollide board tetromino position && position.Y = 0 then true 
    else false

  let public removeLines (board : char list list) =
    let rec iterY (xss : char list list) =
      match xss with
      | [] -> []
      | rh :: rf -> if List.forall (fun x -> x <> ' ') rh then iterY rf else rh :: iterY rf
    iterY board

  let public addLines (size : Point) (board : char list list) =
    let addRows = [ for _ in 0..(size.Y - 1 - (List.length board)) do yield [ for _ in 0..(size.X - 1) do yield ' ' ] ]
    if List.length addRows > 0 then addRows @ board
    else board

  let public calcScore level lines =
    let result = 
      match lines with
      | 1 -> int (40. * (float level + 1.))
      | 2 -> int (40. * 2.5 * (float level + 1.))
      | n -> int (List.fold (fun a e -> a * float e) 100. [3..n] * (float level + 1.))
    int result