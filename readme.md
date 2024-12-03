# ML Agent Brickbreaker

<table>
  <tr>
    <td>
        <img src="/statics/BrickBreaker_normal.gif" width="400" height="400">
        <div align="center"><b>Normal</b></div>
    </td>
    <td>
        <img src="/statics/BrickBreaker_ai_assist.gif" width="400" height="400">
        <div align="center"><b>AI-Assist</b></div>
    </td>
  </tr>
</table>

<div align="center">

**[Unity ML-Agents ToolKit](https://unity-technologies.github.io/ml-agents/) Swipe Brick Breaker Clone Sample-Play**

</div>

---

## 프로젝트 소개
Unity ML Agent를 사용한 Brickbreaker 강화학습 프로젝트입니다. 기본적인 플레이는 스와이프 블럭깨기를 베이스로 하고 있으며. 강화학습 알고리즘을 통해 플레이어를 보조하는 기능을 추가했습니다.

## 주요 기능
- ML Agent 기반 패들 학습
- 강화학습 알고리즘 적용 AI Assist 기능 추가

## 설치 방법
1. Unity 2022.3 LTS 이상 필요
2. ML Agents package 설치
   1. 2.0.1 버전에서 실행했습니다.
   2. 3.0.0 버전은 실험해 보지 않았습니다.
3. 종속성 다운로드

## 실행 방법
- Unity Editor에서 열기
- ML Training 스크립트 실행

## 학습 알고리즘
- Proximal Policy Optimization (PPO)

## 향후 계획
- AI와 VS 모드 제작 후 Web 형태로 배포 예정

## Experiments

가장 Rewards가 높은 모델이 포함되어있으나, 실험했던 Cumulaitve Reward Scalar를 공개

<img src="/statics/Environment_Cumulative Reward.svg">

---

<img src="/statics/specific_rewards.png">
