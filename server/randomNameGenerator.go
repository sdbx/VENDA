package main

import (
	"math/rand"
	"sync/atomic"
	"time"
)

var firstNames = []string{"걷는", "재빠른", "힘쎈", "나태한", "괴랄한", "요염한", "뚜따하는", "끼로끼로", "아싸", "인싸", "호에에"}
var secondNames = []string{"아재", "아저씨", "짱", "군", "아가씨", "님", "선배", "끼로", "교수", "선생", "박사", "할아버지", "할머니", "놈", "자식"}
var thirdNames = []string{"끼로", "작사", "루데브", "프구", "볼트", "알약", "벨붕", "해티", "탄라로", "나비", "도루", "티바이트", "쪼리핑", "플중", "인클", "ㄹ", "비양", "스라", "타자", "개돌이", "볕뉘"}
var randIndice []int
var randIndex uint64

func makeRange(min, max int) []int {
	a := make([]int, max-min+1)
	for i := range a {
		a[i] = min + i
	}
	return a
}

func init() {
	if !(len(thirdNames) >= len(secondNames)) {
		panic("100 > 10")
	}
	rand.Seed(time.Now().UnixNano())
	randIndice = makeRange(0, len(firstNames)*len(secondNames)*len(thirdNames))
	rand.Shuffle(len(randIndice), func(i, j int) { randIndice[i], randIndice[j] = randIndice[j], randIndice[i] })
}

func GenerateName() string {
	ii := randIndice[randIndex]
	a := ii % len(firstNames)
	ii = ii / len(firstNames)
	b := ii % len(secondNames)
	ii = ii / len(secondNames)
	c := ii
	atomic.AddUint64(&randIndex, 1)
	if randIndex == uint64(len(firstNames)*len(secondNames)*len(thirdNames)) {
		atomic.StoreUint64(&randIndex, 0)
	}
	return firstNames[a] + thirdNames[c] + secondNames[b]
}
