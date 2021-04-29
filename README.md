#                 #### Heroes of Galaxy ####
![그림1](https://user-images.githubusercontent.com/58795584/100734769-85475680-3413-11eb-9512-f1cd01fb0cb6.PNG)
+ 팀 프로젝트 Heroes of Galaxy에서 **캐릭터 프로그래밍**을 담당한 신준철입니다. 
+ 저희 팀은 7월부터 12월까지 개발 했으며 지스타, GGC, 학교 경진대회에 작품을 냈습니다.
+ 현재 바르바, 던전 맵 구현을 마쳤습니다.
#### 담당 : 캐릭터 클라이언트
#### 개발기간 : 5개월

------------
# 게임 소개
## FPS 와 RTS 혼합 및 AI 코옵
+ 3명의 NPC와 협동하여 게임을 클리어하는 시스템입니다.
+ NPC는 자율적으로 움직이되, RTS모드로 명령을 내릴 수 있습니다.
+ 3인칭 슈팅과 전략 시뮬레이션의 혼합 장르
+ 스킬과 총을 이용하여 몬스터를 처치합니다.

<img src="https://user-images.githubusercontent.com/58795584/100735990-79f52a80-3415-11eb-99ea-ea2ab9f43e4f.PNG" width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100737782-f852cc00-3417-11eb-84f9-cbd958e41a41.PNG" width="450">
:-------------------------:|:-------------------------:

## 몬스터 및 보스 
+ 각 스테이지에는 1명(기)의 보스가 배치되어 있습니다.
+ '플레이어'는 보스와 전투하기 전 수 많은 일반 몬스터와 조우 및 전투를 하게 됩니다.
+ '플레이어'는 생존한 상태로 보스를 물리쳐야 합니다.

<img src="https://user-images.githubusercontent.com/58795584/100739510-816b0280-341a-11eb-8179-04d22ee1038a.png" width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100739551-921b7880-341a-11eb-8ce2-94349b2f62e9.png" width="450">
:-------------------------:|:-------------------------:

------------
# 구현 내용
## 블렌드 트리, 레이어, 애니메이션 이벤트로 애니메이터 제작
+ 총 4가지의 스테이트로 분류 (서서 라이플, 앉아 라이플, 서서 피스톨, 앉아 라이플)
+ BodyValue라는 변수 4개 구현하여 각각의 스테이트로 가면 1이 되도록 합니다.
+ speed라는 변수를 만들어 수치에 따라 idle, walk, jog, run으로 이동하도록 구현.
+ 또한 레이어를 아바타 전신과 상체로 나누어 총 사격 애니메이션과 이동을 동시에 실행시키도록 만들었습니다.
+ 애니메이션 이벤트를 이용해 특정 애니메이션 시간대에 함수가 호출되도록 하였습니다.
+ 무기 교체 및 스킬 등에 애니메이션 이벤트를 사용.

<img src="https://user-images.githubusercontent.com/58795584/100778665-a2e3e280-344a-11eb-8cb4-5ac101b65133.PNG"  width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100778694-aaa38700-344a-11eb-87b0-b42c9b6aa3c9.PNG"  width="450"> | 
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/58795584/100778930-fa824e00-344a-11eb-971a-b44d4ac7de2f.PNG"  width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100778956-02da8900-344b-11eb-8e28-bd423feda774.PNG"  width="450">

## 캐릭터 컨트롤러를 이용한 캐릭터 구현
* 고질적인 문제인 캡슐 콜라이더의 isGrounded 충돌 체크를 5줄의 빨간 Ray로 
* Slope Sliding을 구현. (Red Ray 이용하여 오브젝트를 검출. green은 오브젝트의 노멀 벡터 Yellow는 내려가야할 각도를 그려냄)
* Angle 함수로 캐릭터의 up벡터를 기준으로 충돌된 오브젝트의 normal vec와의 각도를 계산하여 경사각을 구합니다.
* 경사각이 제한보다 크다면 Slope Sliding이 실행됩니다.

<img src="https://user-images.githubusercontent.com/77636255/116537490-45808e80-a921-11eb-92f6-e982fce07066.png"  width="550" height="300"> | <img src="https://user-images.githubusercontent.com/77636255/116537525-50d3ba00-a921-11eb-92e8-6b5b62dfe7b7.png"  width="550">
:-------------------------:|:-------------------------:
```C#
  public void CheckGround()
  {
      // 캐릭터 컴포넌트 Null Check
      if (body == null)
      {
          return;
      }

      // 5개의 Ray 생성
      Ray[] modify = initRay();

      RaycastHit hit;
      int hitcount = 0;

      // FixedUpdate를 통해 실행 (Update 보다 먼저 실행된다.)
      for (int i = 0; i < 5; ++i)
      {
          Debug.DrawRay(modify[i].origin, modify[i].direction * 0.3f, Color.red);
          if (Physics.Raycast(modify[i], out hit, 0.3f))
          {
              // Hit Check
              ++hitcount;

              // 가운데에 있는 Ray를 사용
              if (i == 0)
              {
                  // 캐릭터 중심으로 레이를 쏴 현재 밟는 땅의 경사 각도를 알아낸다.
                  groundslopeAngle = Vector3.Angle(transform.up, hit.normal);
                  // 캐릭터 컨트롤러의 경사 제한보다 크다면 경사를 오르지 못하게 한다. (슬라이딩)
                  if(groundslopeAngle > body.slopeLimit)
                  {
                      // 외적을 이용하여 법선 벡터를 구한다. (캐릭터의 x축을 지나는 벡터가 구해진다.)
                      Vector3 groundCross = Vector3.Cross(hit.normal, Vector3.up);
                      // 법선벡터 (x축)와 충돌한 땅의 노멀벡터와 수직인 외적 법선 벡터를 구해서 떨어져야 할 각도를 구해낸다. 
                      // 캐릭터의 윗방향 수직인 벡터와 외적으로 구해낸 경사각까지의 각도를 fallDirection에 대입.
                      fallDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, hit.normal));

                      Debug.DrawRay(transform.position, -fallDirection.up, Color.yellow);
                      return;
                  }

                  // 오를 수 있는 경사
                  else
                  {
                      fallDirection.eulerAngles = Vector3.zero;
                  }
              }

              // 바닥에 있는지 체크
              if (groundslopeAngle <= body.slopeLimit && body.isGrounded && hitcount > 0)
              {
                  isGrounded = true;
                  // CharacterComponent.Move()에 사용 될 Y 값을 중력 처럼 넣어준다.
                  yVelocity = gravityY * gravityMultiplexer * Second;
              }
          }
      }

      // 레이가 하나도 충돌 되지 않았다면 공중에 있다고 판별
      if (hitcount == 0)
      {
          isGrounded = false;
      }
  }
```
## 3인칭 카메라 콜리전 구현
+ 카메라를 똑같이 따라가는 카메라 더미 하나를 생성
+ 충돌 검사를 카메라 더미로 실행
+ 충돌 시 더미는 계속 충돌 검사하며 실제 카메라는 충돌한 포인트로 위치를 옮긴다.

<img src="https://user-images.githubusercontent.com/77636255/116544244-d3f90e00-a929-11eb-92b2-67a194b7624d.png"  width="450" height="270"> | <img src="https://user-images.githubusercontent.com/77636255/116544266-df4c3980-a929-11eb-9d08-36620e2d2f26.png"  width="450">
:-------------------------:|:-------------------------:

```C#
  // Target은 화면의 정중앙, CameraTransform은 더미
            Ray ray = new Ray(Target.position, CameraTransform.transform.position - Target.position);
            float distance = Vector3.Distance(CameraTransform.transform.position, Target.position);
            if(Physics.Raycast(ray, out hit, distance + 0.5f, collisionLayer) && player.p_Astate != AIM_STATE.ZOOM)
            {
                transform.position = hit.point;
                // modify는 캐릭터를 기준 (0,0,0) 으로 카메라까지의 방향
                // 카메라는 캐릭터의 뒤로 땡겨져있다. 그 반대는 캐릭터의 앞으로...
                transform.Translate(-modify * 0.5f);
            }
            else
            {
                cameraPositionSetting();
            }
```

## IK 시스템 사용
+ 다양한 총기의 모션을 위해 IK 시스템을 이용하여 원하는 위치에 왼손과 오른손을 두도록 만들었습니다.
+ 각각의 총기마다 오른손과 왼손의 위치가 존재하며 json으로 관리하고 있습니다.
+ 캐릭터의 상체가 크로스헤어, 카메라 정중앙을 바라보도록 만들었습니다.

<img src="https://user-images.githubusercontent.com/58795584/100779905-6add9f00-344c-11eb-8f3b-85a5a2eb0343.PNG"  width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100779920-7204ad00-344c-11eb-80c5-94656400e34f.PNG"  width="450">
:-------------------------:|:-------------------------:

#### 카메라가 바라보는 방향에 따라 캐릭터의 바라보는 방향이 바뀐다.

<img src="https://user-images.githubusercontent.com/58795584/100779342-9744eb80-344b-11eb-8366-1964bdfea184.PNG"  width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100779044-23a2de80-344b-11eb-9fb9-cece3dd58ba3.PNG"  width="450">
:-------------------------:|:-------------------------:
<img src="https://user-images.githubusercontent.com/58795584/100779027-1d146700-344b-11eb-9fd9-49eea01fb59d.PNG"  width="450"> | <img src="https://user-images.githubusercontent.com/58795584/100779505-d5420f80-344b-11eb-87c1-39c3ee0edc8b.PNG"  width="450">

#### 총기에 맞는 애니메이션을 구현

```C#
    // HumanBones.RightArm을 shoulder 변수로 만듬.
                    aimPivot.position = shoulder.position;
                    anim.SetLookAtWeight(1, 0.2f, 1);

                    aimPivot.LookAt(targetLook.position);
                    anim.SetLookAtPosition(targetLook.position);

                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, lh_Weight);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, lh_Weight);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, l_Hand.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, lh_rot);

                    anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rh_Weight);
                    anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rh_Weight);
                    anim.SetIKPosition(AvatarIKGoal.RightHand, r_Hand.position);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, r_Hand.rotation);
```
![무기오브젝트](https://user-images.githubusercontent.com/77636255/116544599-51248300-a92a-11eb-923c-ad482582f74d.png)

## 오브젝트 풀링
+ 오브젝트 풀링은 스킬에 사용되는 이펙트, 사격 시 떨어지는 탄피, 장전 시 떨어지는 탄창에 사용되었습니다.
+ 싱글톤을 이용하여 오브젝트 풀링을 관리했습니다.
+ 게임 로딩시간에 선택한 무기와 스킬에 맞는 이펙트와 총알들을 모두 로드 시킨 뒤, 오브젝트들을 Queue에 Push 후 false 시켰습니다.
+ 사용 시엔 Queue에서 Pop시켜 사용하며 일정 시간이 지나거나 스킬이 끝나면 Pop시킨 오브젝트를 다시 Push시켜 재사용합니다.
+ string과 Queue로 구성된 딕셔너리 m_ist를 만들어 오브젝트들을 종류에 따라 관리하도록 만들었습니다.

* 반납 하기
```C#
 public void InsertObject(string key, GameObject p_obj)
    {
        // string과 Queue로 구성된 Dictionary m_list를 생성
        // string으로 종류에 따른 오브젝트를 따로 관리
        m_list[key].Enqueue(p_obj);

        // 사격 후 떨어지는 탄피들에게 없는 경우 Rigidbody를 생성.
        if(p_obj.GetComponent<Rigidbody>() == null)
        {
            p_obj.AddComponent<Rigidbody>();
        }

        p_obj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // MagazinePool 오브젝트의 자식으로 넣는다.
        p_obj.transform.SetParent(this.transform);
        // false...
        p_obj.SetActive(false);
    }
```

* 꺼내 쓰기
```C#
 public GameObject PullObject(string key)
    {
        // 꺼낼려 하는데 없는 경우 새로 만든다.
        if(m_list[key].Count == 0)
        {
            GameObject ammo = Instantiate(dic[key]);
            InsertObject(key, ammo);

            AutoObjectDestroying b = ammo.AddComponent<AutoObjectDestroying>();
            b.time = 5f;
            b.name = key;
            b.destroyingWaitTime = new WaitForSeconds(b.time);
        }

        // 오브젝트를 꺼낸다.   
        GameObject t_obj = m_list[key].Dequeue();
        t_obj.SetActive(true);
        t_obj.transform.SetParent(null);

        return t_obj;
    }
```
![오브젝트풀링](https://user-images.githubusercontent.com/77636255/116544747-7e713100-a92a-11eb-9475-57960f600b1b.png)

#####  쓰는 오브젝트들을 하나의 부모의 자식으로 넣되, 딕셔너리를 이용해 따로따로 관리

<img src="https://user-images.githubusercontent.com/58795584/101144613-eeb9a600-365b-11eb-81d8-b635e24095ba.PNG"  width="600"> | <img src="https://user-images.githubusercontent.com/58795584/101144636-f7aa7780-365b-11eb-8038-38db59d7c244.gif"  width="600">
:-------------------------:|:-------------------------:

##### 대표적으로 사격 후 떨어지는 탄피들에 사용했다.

## JSON 데이터 세팅
![오브젝트풀링](https://user-images.githubusercontent.com/77636255/116545009-d4de6f80-a92a-11eb-84a8-5c33a7e325cc.png)

## 커스텀 에디터
```c#
[CustomEditor(typeof(SkillManager))]
public class SkillEditor : Editor
{
    private bool[] SkillPerk = new bool[9];

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SkillManager perkTest = (SkillManager)target;

        GUILayout.Space(15);
        GUILayout.Label("Skill Perk", EditorStyles.boldLabel);
        SkillPerk[0] = EditorGUILayout.Toggle("폭렬 대쉬", SkillPerk[0]);
        SkillPerk[1] = EditorGUILayout.Toggle("클라이맥스", SkillPerk[1]);
        SkillPerk[2] = EditorGUILayout.Toggle("정오의 주인공", SkillPerk[2]);
        SkillPerk[3] = EditorGUILayout.Toggle("사이킥 폭풍", SkillPerk[3]);
        SkillPerk[4] = EditorGUILayout.Toggle("염동력", SkillPerk[4]);
        SkillPerk[5] = EditorGUILayout.Toggle("망령 탄환", SkillPerk[5]);
        SkillPerk[6] = EditorGUILayout.Toggle("무차별 포화", SkillPerk[6]);
        SkillPerk[7] = EditorGUILayout.Toggle("바람 구멍", SkillPerk[7]);
        SkillPerk[8] = EditorGUILayout.Toggle("폭탄 뿌리기", SkillPerk[8]);

        if(GUILayout.Button("캐릭터에 퍽 적용"))
        {
            if(!Application.isPlaying)
            {
                Debug.Log("Play중에만 적용 가능 합니다.");
                return;
            }


            for(int i = 0; i < SkillPerk.Length; i++)
            {
                if(SkillPerk[i])
                {
                    perkTest.Initialized(2000 + i);
                }
            }
        }
    }
}
```
## 스킬 구현 및 기타
+ 캐릭터의 스펙을 데이터화 하기 위해 로딩 시간에 캐릭터를 구현하는데 필요한 모든 컴포넌트를 코드로 제작합니다. (GameManager)
+ 플레이어 매니저 스크립트를 따로 만들어 플레이어 구동에 필요한 코드들을 관리 및 서순을 정리했습니다. (Player_Manager)
+ 게임 홍보 영상입니다. [영상 링크](https://youtu.be/nFbnCIlHbpg)
+ 플레이어 스킬 영상입니다.
+ [폭렬 대쉬](https://youtu.be/aIDAlNDNzYI), [클라이맥스](https://youtu.be/iPaevBxKvN0), [정오의 주인공](https://youtu.be/sCfBqqwG5-Q), [사이킥 폭풍](https://youtu.be/dERqsuWcYs0), [염동력](https://youtu.be/FS85jic2FrI), [무차별 탄환](https://youtu.be/-v3yz9zgfQo), [바람 구멍](https://youtu.be/-QsGkroOFCw), [폭탄 뿌리기](https://youtu.be/6d7SaL7PygM)

## 팀 프로젝트를 하면서..
+ 그래픽을 담당하는 분이 팀에 있지 않아 애니메이션을 이용한 프로그래밍 작업이 힘들었습니다. 특히나 하나의 모션으로 다양한 총기 모션을 표현해야했기 때문에 팀원과의 대화를 통해 하나의 모션을 이용해 유니티의 IK 시스템을 이용하여 손과 팔목을 자유자재로 움직일 수 있도록 만들어봤습니다.
