from PIL import Image
import numpy as np

def crop_alpha_by_radius(
    input_path,
    output_path,
    radius=450
):
    img = Image.open(input_path).convert("RGBA")
    data = np.array(img)

    h, w = data.shape[:2]

    # 图片中心
    cx = w / 2.0
    cy = h / 2.0

    # 像素坐标网格
    y, x = np.ogrid[:h, :w]

    # 到中心的距离平方
    dist2 = (x - cx) ** 2 + (y - cy) ** 2

    # 圆外区域
    outside = dist2 > radius * radius

    # Alpha = 0
    data[outside, 3] = 0

    Image.fromarray(data, "RGBA").save(output_path)


if __name__ == "__main__":
    crop_alpha_by_radius(
        "2.png",
        "2_.png",
        radius=315
    )