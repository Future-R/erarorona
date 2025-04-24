import csv
import tkinter as tk
from tkinter import font

def load_map_data():
    with open('地图.csv', 'r', encoding='utf-8') as f:
        return list(csv.reader(f, delimiter=','))

def load_location_data():
    locations = {}
    with open('位置产物.csv', 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            loc_id = row['位置编号'].zfill(2).upper()
            locations[loc_id] = row
    return locations

class Application(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("地图浏览器")
        self.geometry("1300x820")
        self.resizable(False, False)
        
        # 加载数据
        self.map_data = load_map_data()
        self.locations = load_location_data()
        
        # 创建字体
        self.custom_font = font.Font(family='等线', size=14)
        self.symbol_font = font.Font(family='SimSun', size=14, weight='bold')
        
        # 创建界面布局
        self.create_widgets()
        
    def create_widgets(self):
        # 左侧地图画布
        self.canvas = tk.Canvas(self, width=980, height=820, bg='white')
        self.canvas.pack(side=tk.LEFT)
        
        # 右侧信息框
        self.info_text = tk.Text(self, width=39, height=51, font=self.custom_font)
        self.info_text.pack(side=tk.RIGHT, padx=10)
        
        # 绘制地图
        self.draw_map()
    
    def draw_map(self):
        cell_size = 20
        for y, row in enumerate(self.map_data):
            for x, cell in enumerate(row):
                x1 = x * cell_size
                y1 = y * cell_size
                x2 = x1 + cell_size
                y2 = y1 + cell_size
                
                # 绘制网格线
                self.canvas.create_rectangle(x1, y1, x2, y2, outline='white')
                
                cell = cell.strip()
                if len(cell) == 2 and all(c in '0123456789ABCDEF' for c in cell):
                    self.create_location_button(x1, y1, x2, y2, cell)
                elif cell in ('─', '│'):
                    self.draw_line(x1, y1, cell)
    
    def create_location_button(self, x1, y1, x2, y2, loc_id):
        # 获取位置信息
        info = self.locations.get(loc_id.upper())
        symbol = "△"  # 默认未找到
        
        if info:
            loc_name = info['位置名称']
            if loc_name == "亚兰德":
                symbol = "☆"
            elif loc_name == "阿拉尼亚村":
                symbol = "○"
            else:
                symbol = "□"
        
        # 绘制符号（居中显示）
        text_x = (x1 + x2) // 2
        text_y = (y1 + y2) // 2
        self.canvas.create_text(
            text_x, text_y, 
            text=symbol, 
            font=self.symbol_font, 
            fill='blue' if symbol != "△" else 'green'
        )
        
        # 创建透明点击区域
        rect = self.canvas.create_rectangle(x1+1, y1+1, x2-1, y2-1, fill='', outline='')
        self.canvas.tag_bind(rect, '<Button-1>', 
            lambda e, id=loc_id: self.show_location_info(id))
    
    def draw_line(self, x1, y1, symbol):
        x2 = x1 + 20
        y2 = y1 + 20
        if symbol == '─':
            self.canvas.create_line(x1, y1+10, x2, y1+10, fill='black',width=3)
        elif symbol == '│':
            self.canvas.create_line(x1+10, y1, x1+10, y2, fill='black',width=3)
    
    def show_location_info(self, loc_id):
        info = self.locations.get(loc_id.upper())
        self.info_text.delete(1.0, tk.END)
        
        if not info:
            self.info_text.insert(tk.END, "分岔路")
            return
        
        # 基本信息
        self.info_text.insert(tk.END, 
            f"{info['所属地图']}/{info['位置名称']}\n\n采集结果　　　　几率　品质范围\n\n")
        
        # 产物信息
        total_rate = 0
        for i in range(1, 7):
            product = info[f'产物{i}']
            if not product: continue
            
            rate = info[f'出现率{i}']
            total_rate += int(rate or 0)
            low = info[f'最低{i}']
            high = info[f'最高{i}']
            
            line = f"{product.ljust(8,'　')} {rate}%　({low}~{high})\n"
            self.info_text.insert(tk.END, line)
        
        # 失败率
        fail_rate = 100 - total_rate
        self.info_text.insert(tk.END, 
            f"\n{'什么都没有'.ljust(8,'　')} {info['无产物率']}%\n\n")
        
        # 稀有产物
        for i in range(1, 3):
            rare = info.get(f'稀有{i}')
            if rare:
                self.info_text.insert(tk.END, "*" + rare + "\n")

if __name__ == '__main__':
    app = Application()
    app.mainloop()