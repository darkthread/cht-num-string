import re

class ChtNumConverter:
    def __init__(self):
        self.ChtNums = "零一二三四五六七八九"
        self.ChtUnits = {
            "十": 10,
            "百": 100,
            "千": 1000,
            "萬": 10000,
            "億": 100000000,
            "兆": 1000000000000
        }

    def ParseChtNum(self, chtNumString):
        isNegative = False
        if chtNumString.startswith("負"):
            chtNumString = chtNumString[1:]
            isNegative = True
        num = 0

        def Parse4Digits(s):
            lastDigit = 0
            subNum = 0
            for rawChar in s:
                c = rawChar.replace("〇", "零")
                if c in self.ChtNums:
                    lastDigit = self.ChtNums.index(c)
                elif c in self.ChtUnits:
                    if c == "十" and lastDigit == 0:
                        lastDigit = 1
                    unit = self.ChtUnits[c]
                    subNum += lastDigit * unit
                    lastDigit = 0
                else:
                    raise ValueError(f"Contains unparsable Chinese numerals: {c}")
            subNum += lastDigit
            return subNum

        for splitUnit in "兆億萬":
            pos = chtNumString.find(splitUnit)
            if pos == -1:
                continue
            subNumString = chtNumString[:pos]
            chtNumString = chtNumString[pos + 1:]
            num += Parse4Digits(subNumString) * self.ChtUnits[splitUnit]
        num += Parse4Digits(chtNumString)
        return -num if isNegative else num

    def ToChtNum(self, n):
        negative = n < 0
        if negative:
            n = -n
        if n >= 10000 * self.ChtUnits["兆"]:
            raise ValueError("Number exceeds convertible range")
        unitChars = list("千百十")

        def Conv4Digits(subNum):
            sb = ''
            for c in unitChars:
                if subNum >= self.ChtUnits[c]:
                    digit = subNum // self.ChtUnits[c]
                    subNum = subNum % self.ChtUnits[c]
                    sb += f"{self.ChtNums[digit]}{c}"
                else:
                    sb += "零"
            sb += self.ChtNums[subNum]
            return sb

        numString = ''
        forceRun = False
        for splitUnit in "兆億萬":
            unit = self.ChtUnits[splitUnit]
            if n < unit:
                if forceRun:
                    numString += "零"
                continue
            forceRun = True
            subNum = n // unit
            n = n % unit
            if subNum > 0:
                numString += Conv4Digits(subNum).rstrip('零') + splitUnit
            else:
                numString += "零"
        numString += Conv4Digits(n)
        t = re.sub("零+", "零", numString)
        if len(t) > 1:
            t = t.rstrip("零").lstrip("零")
        t = re.sub("^一十", "十", t)
        return ("負" if negative else "") + t


numSets = [
    "0000", "0001", "0010", "0012",
    "0100", "0102", "0310", "0123",
    "1000", "1002", "1020", "1023",
    "1200", "1203", "1230", "1234"
]

count = len(numSets)
idxs = [0, 0, 0, 0]
lv = len(idxs) - 1

converter = ChtNumConverter()

while True:
    sb = ''.join(numSets[i] for i in idxs)
    n = int(sb)
    cht = converter.ToChtNum(n)
    restored = converter.ParseChtNum(cht)
    print(f"{n} {restored} " + ("PASS" if n == restored else "FAIL") + f" {cht} ")
    if n != restored:
        raise ValueError(f"Number conversion error {n} vs {restored}")
    if all(o == count - 1 for o in idxs):
        break
    idxs[lv] += 1
    while idxs[lv] == count:
        idxs[lv] = 0
        lv -= 1
        if lv < 0:
            break
        idxs[lv] += 1
    lv = len(idxs) - 1