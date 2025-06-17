using Godot;
using System;
using System.Collections.Generic;

public class Tile {
    public int Number { set; get; }
    public bool Bomb { set; get; }
    public bool Flag { set; get; }
    public bool Hidden { set; get; }
}
public partial class Minesweeper : Control {
    [ Export ] public Font font;
    public List<List<Tile>> Board = new();
    private int Zoom = 1;
    private Vector2I revealAllTiles = Vector2I.Zero;
    private bool startRevealAllTiles = false;
    private float time = 0;

    public override void _Ready() {
        foreach ( MyButton button in GetNode<HBoxContainer>( "HBoxContainer" ).GetChildren() ) {
            button.Pressed += () => {
                ZoomGame( button.ButtonType );
            };
        };
        GetNode<MyButton>( "Button" ).Pressed += () => {
            GetParent().GetNode<VBoxContainer>( "VBoxContainer" ).Visible = true;
        };
        SetProcess( false );
    }
    
    public override void _Process(double delta) {
        if ( startRevealAllTiles ) {
            time += ( float ) delta;
            if ( time > ( 1 / 225 ) ) {
                RevealAllTiles();
                time = 0;
            }
        }

        QueueRedraw();
    }
    public override void _Draw() {
        int tileSize = 16 * Zoom;
        Vector2I screenSize = DisplayServer.WindowGetSize();
        Vector2 centerBoard = new Vector2(screenSize.X / 2 - Board.Count * tileSize / 2, screenSize.Y / 2 - Board.Count * tileSize / 2);

        for (int y = 0; y < Board.Count; y++) {
            for (int x = 0; x < Board[y].Count; x++) {
                Tile tile = Board[y][x];

                DrawRect(
                    new Rect2(new Vector2(centerBoard.X + x * tileSize + 2, centerBoard.Y + y * tileSize + 2), new Vector2(tileSize - 2, tileSize - 2)),
                    tile.Bomb && !tile.Hidden ? new Color(0.545098f, 0, 0, 1) : new Color(0.745098f, 0.745098f, 0.745098f, 1),
                    filled: true
                );

                //If tile is revealed draw number
                if (!tile.Hidden) {
                    if (tile.Bomb) {
                        // If it's a bomb

                    }

                    else if (tile.Number > 0) {

                        //numb to string
                        string numberChar = tile.Number.ToString();

                        Vector2 stringSize = font.GetStringSize( numberChar, HorizontalAlignment.Center, -1, 8 * Zoom );
                        Vector2 drawPos = new Vector2(centerBoard.X + x * tileSize + tileSize / 2 - stringSize.X / 2, centerBoard.Y + y * tileSize + tileSize / 2 + stringSize.Y / 2.5f);

                        // Draw the number centered on the tile
                        DrawString( font, drawPos, numberChar, HorizontalAlignment.Center, -1, 8 * Zoom );
                    }
                }

            }
        }
    }

    public override void _Input(InputEvent @event) {
        if ( @event is InputEventMouseButton mouseInput ) {

            int tileSize = 16 * Zoom;

            Vector2I screenSize = DisplayServer.WindowGetSize();
            Vector2 centerBoard = new Vector2(screenSize.X / 2 - Board.Count * tileSize / 2, screenSize.Y / 2 - Board.Count * tileSize / 2);
            Vector2I tileIndex = ( Vector2I ) ( ( mouseInput.Position - centerBoard ) / tileSize ).Floor();
            
            if ( tileIndex.X >= 0 && tileIndex.X < 15 && tileIndex.Y >= 0 && tileIndex.Y < 15 ) RevealTile( tileIndex );
        }
    }

    public void SetupGame(int Bombs) {
        Board = new();
        revealAllTiles = Vector2I.Zero;
        startRevealAllTiles = false;
        time = 0;

        for (int i = 0; i < 15; i++) {
            Board.Add(new List<Tile>());
            for (int j = 0; j < 15; j++) {
                Board[i].Add(new Tile() {
                    Number = 0,
                    Bomb = false,
                    Flag = false,
                    Hidden = true
                });
            }
        }

        for (int i = 0; i < Bombs;) {
            int x = GD.RandRange(0, 14);
            int y = GD.RandRange(0, 14);

            if (Board[x][y].Bomb) continue;
            Board[x][y].Bomb = true;

            // double for loop
            // add an integer around the bomb area 

            for (int k = -1; k <= 1; k++) for (int h = -1; h <= 1; h++) {
                if (k + x < 0 || k + x >= 15 || h + y < 0 || h + y >= 15) continue;

                Board[k + x][h + y].Number++;
            }

            i++;
        }

        SetProcess(true);
    }

    private void RevealTile( Vector2I position ) {
        Tile tile = Board[ position.Y ][ position.X ];

        if ( !tile.Hidden ) return;
        if ( tile.Bomb ) {
            RevealAllTiles();
            startRevealAllTiles = true;
            return;
        }

        tile.Hidden = false;

        if ( tile.Number == 0 ) {
            for (int k = -1; k <= 1; k++) for (int h = -1; h <= 1; h++) {
                if (k + position.X < 0 || k + position.X >= 15 || h + position.Y < 0 || h + position.Y >= 15) continue;

                RevealTile(new Vector2I(k + position.X, h + position.Y));
            }
        }
    }

    private void RevealAllTiles() {
        Board[ revealAllTiles.Y ][ revealAllTiles.X ].Hidden = false;

        revealAllTiles.X++;

        if ( revealAllTiles.X >= 15 ) {
            revealAllTiles.X = 0;
            revealAllTiles.Y++;
        }

        if ( revealAllTiles.Y >= 15 ) {
            startRevealAllTiles = false;
        }
    }

    private void ZoomGame( MyButton.ButtonTypeEnum buttonType ) {
        switch ( buttonType ) {
            case MyButton.ButtonTypeEnum.ZoomIn:
                Zoom = Math.Clamp( Zoom + 1, 1, 10 );
                break;
            case MyButton.ButtonTypeEnum.ZoomOut:
                Zoom = Math.Clamp( Zoom - 1, 1, 10 );
                break;
        }
    }
}
    