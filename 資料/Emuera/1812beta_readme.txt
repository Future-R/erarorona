1812
・HTML_PRINT命令に<nobutton>～</nobutton>タグ追加。後述のtitle属性用
・HTML_PRINT命令で<button>タグのvalue属性を省略すると（非ボタン<nobutton>と同等）として扱われるように
・ButtonへのToolTipの追加
　<button>ボタンタグ、又は<nonbutton>タグにtitle属性を指定することでツールチップを表示できます
・TOOLTIP_SETCOLOR命令、TOOLTIP_DELAY命令の追加
・COLOR_FROMNAME(str)関数、COLOR_FROMRGB(int,int,int)関数の追加
・macro.txtを編集することでマクログループを変更したときの表示文字列を変更できるように
・文字列式を用いた文字列変数への代入演算子 '= を追加
・識別子が適切でない場合のエラーメッセージを若干修正


ツールチップについて、
HTML_PRINT "<nonbutton title='あいうえ'>ほげほげ</nobutton>
これで「ほげほげ」の上にマウスポインタを持っていくと「あいうえ」とのツールチップが表示されます
TOOLTIP_SETCOLOR命令
　TOOLTIP_SETCOLOR int 文字色, int 背景色
　ツールチップの文字色と背景色を整数値で指定します
　指定にR,G,B値や文字列を使いたい場合は後述の関数群を利用してください
TOOLTIP_DELAY命令
　TOOLTIP_DELAY int ミリ秒
　ポイントしてからツールチップを表示するまでの時間をミリ秒単位で指定します
　ディフォルトは500(ミリ秒)です
COLOR_FROMNAME(str)関数
　COLOR_FROMNAME(str 色名)
　色名から色を表す整数値を取得します
　対応する色名が無い場合、負の値を返します
COLOR_FROMRGB(int,int,int)関数
　COLOR_FROMRGB(int R, int G, int B)
　R、G、B値から色を表す整数値を取得します
　各値に0～255の範囲外の値を渡すとエラーになります
　この関数の戻り値は(R*0x10000 + G * 0x100 + B)の結果と等しくなります


文字列式を用いた文字列変数への代入文'=
今までの代入文がPRINTFORMとすればこれはPRINTS相当の文法で代入する代入文です
文字列定数を代入するには"～～"で囲む必要がありますが、変数や式の代入はより直感的に行うことができます
NAME:MASTER = ああああ
NAME:MASTER = %LOCALS%
NAME:MASTER '= "ああああ"
NAME:MASTER '= LOCALS

数値の代入と同じく配列変数に複数の値を同時に代入することも可能です
STR '= "ACB", "BCD", "CDE"

