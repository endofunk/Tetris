//
// Tetromino.fs
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
open System
open Microsoft.Xna.Framework

[<AutoOpen>]
module public Tetromino =
  let public leftPeriscope =
    [ [ ' '; 'J' ]
      [ ' '; 'J' ]
      [ 'J'; 'J' ] ]
      
  let public rightPeriscope =
    [ [ 'L'; ' ' ]
      [ 'L'; ' ' ]
      [ 'L'; 'L' ] ]
      
  let public stick =
    [ [ 'I' ]
      [ 'I' ]
      [ 'I' ]
      [ 'I' ] ]
      
  let public leftDog =
    [ [ 'Z'; 'Z'; ' ' ]
      [ ' '; 'Z'; 'Z' ] ]
      
  let public rightDog =
    [ [ ' '; 'S'; 'S' ]
      [ 'S'; 'S'; ' ' ] ]
      
  let public square =
    [ [ 'O'; 'O' ]
      [ 'O'; 'O' ] ]
      
  let public tee =
    [ [ ' '; 'T'; ' ' ]
      [ 'T'; 'T'; 'T' ] ]
  
  let public Tetrominos = 
    let r = Random()
    let parts = [ leftPeriscope; rightPeriscope; stick; leftDog; rightDog; square; tee ]
    Seq.initInfinite(fun _ -> parts.[r.Next(0, List.length parts)])
    
  let public char2Color c =
    match c with
    | 'J' -> Color.Blue
    | 'L' -> Color.DarkOrange
    | 'I' -> Color.DeepSkyBlue
    | 'Z' -> Color.Red
    | 'S' -> Color.LimeGreen
    | 'O' -> Color.Yellow
    | 'T' -> Color.Magenta
    | ' ' -> Color.Transparent
    | _ -> Color.Beige