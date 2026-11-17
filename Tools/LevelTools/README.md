# Level tools (Python 3, standard library only)

The three levels are ASCII text files in `Assets/_Game/Levels/` (one character per 16×16 tile). Unity's
level builder turns them into scenes. These scripts help create and check those files.

- `levels.py OUT_DIR PREVIEW_DIR`: writes the three layouts from code (`OUT_DIR` = `Assets/_Game/Levels`) and
  colour previews. The `.txt` files in the project currently match its output exactly.
- `reach.py LEVELS_DIR`: conservative reachability check (jump ≤ 3 tiles up / ≤ 5 across, ladders, one-way
  ledges, gates opened in key order, every story marker). Run it after editing a layout.
- `png.py`: a tiny PNG reader/writer used by the above (also `zoom` / `runs` / `grid` helpers for slicing art).

**Which file to edit.** Either edit the `.txt` layouts by hand, or edit `levels.py` and regenerate them.
Don't mix the two: running `levels.py` overwrites hand edits in the `.txt` files.

Example (from the project root):

    python Tools/LevelTools/reach.py Assets/_Game/Levels

After changing a layout, run **Tools → The Last Memory → Rebuild Game** in Unity (or the batch command in
`Docs/STUDENT_PROJECT_GUIDE.md`). The symbols are listed in `Docs/GAME_DESIGN.md` §9.
