from PIL import Image
import numpy as np

def remove_only_black_background(input_path, output_path,
                                 threshold=17):
    """
    仅移除纯黑/接近黑背景，不影响其他颜色
    适合游戏UI sprite
    """

    img = Image.open(input_path).convert("RGBA")
    data = np.array(img)

    r, g, b, a = data[:,:,0], data[:,:,1], data[:,:,2], data[:,:,3]

    # ----------------------------
    # 只判断“黑色背景”
    # ----------------------------
    black_mask = (r <= threshold) & (g <= threshold) & (b <= threshold)

    # ----------------------------
    # 设置 alpha = 0（透明）
    # ----------------------------
    data[black_mask, 3] = 0

    # ----------------------------
    # 保存
    # ----------------------------
    out = Image.fromarray(data, "RGBA")
    out.save(output_path, "PNG")


if __name__ == "__main__":
    input_file = "use_button.png"
    output_file = "use_button_1.png"

    remove_only_black_background(input_file, output_file)

    input_file = "discard_button.png"
    output_file = "discard_button_1.png"
    remove_only_black_background(input_file, output_file)

    print("Done ->", output_file)