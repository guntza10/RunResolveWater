# Resolve IsGovernmentUsage And IsGovernmentWaterQuality

**ให้ group โดยใช้ EA เป็น key ก่อน group โดยใช้ road เป็น Key อีกครั้ง**

## โจทย์
### Resolve IsGovernmentUsage

* ถ้า IsGovernmentUsage ใดๆ ภายใน road เป็น 1 ให้ set IsGovernmentUsage ทั้งหมด เป็น 1

### Resolve IsGovernmentWaterQuality

* ถ้า IsGovernmentWaterQuality ใดๆ ภายใน road เป็น 1 ให้ set IsGovernmentUsage ทั้งหมด เป็น 1

---

* กรณีที่ 1

|        |IsGovernmentUsage|IsGovernmentWaterQuality|
|:------:|:---------------:|:----------------------:|
| unit 1 |       1         |            0           |
| unit 2 |       1         |            0           |
| unit 3 |       0         |            0           |
| unit 4 |       0         |            0           |
| unit 5 |       0         |            0           |

* ให้เปลี่ยนเฉพาะ IsGovernmentUsage ทั้งหมดให้เป็น 1 เท่านั้น
* ไม่ต้องเปลี่ยนค่าของ IsGovernmentWaterQuality

---

* กรณีที่ 2

|        |IsGovernmentUsage|IsGovernmentWaterQuality|
|:------:|:---------------:|:----------------------:|
| unit 1 |       1         |            1           |
| unit 2 |       1         |            0           |
| unit 3 |       0         |            0           |
| unit 4 |       0         |            0           |
| unit 5 |       0         |            0           |

* ให้เปลี่ยนเฉพาะ IsGovernmentUsage และ IsGovernmentWaterQuality ทั้งหมดให้เป็น 1

---

* กรณีที่ 3

|        |IsGovernmentUsage|IsGovernmentWaterQuality|
|:------:|:---------------:|:----------------------:|
| unit 1 |       0         |            0           |
| unit 2 |       0         |            0           |
| unit 3 |       0         |            0           |
| unit 4 |       0         |            0           |
| unit 5 |       0         |            0           |

* ไม่ต้องทำการเปลี่ยนใด ๆ