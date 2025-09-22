using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapObject : MonoBehaviour {
    [SerializeField] private Transform visual;
    [SerializeField] private AnimationCurve easeIn;
    [SerializeField] private AnimationCurve easeOut;
    [SerializeField] private MapObjectData data;

    
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D collider;
    private float duration = 0.3f;
    private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private int rotationIndex = 0;
    public int RotationIndex => rotationIndex;

    private void OnEnable() {
        // 테스트용
        Init();
        PlayAppear();
    }

    // 테스트용
    private void Update() {
        if (InputManager.GetKeyDown(KeyAction.Jump)) {
            RotateNext();
            PlayAppear();
        }
    }

    public void Init(MapObjectData newData = null) {
        if(newData != null) {
            data = newData;
        }

        // 스프라이트 적용
        if (data.Sprites != null && data.Sprites.Count > 0) {
            spriteRenderer.sprite = data.Sprites[rotationIndex];
        }
        ApplyRotation();

        // 콜라이더 적용
        ApplyCollider();

        // 추가 컴포넌트 붙이기
        if (data.AdditionalComponents != null) {
            foreach (string compName in data.AdditionalComponents) {
                var type = System.Type.GetType(compName);
                if (type != null && GetComponent(type) == null) {
                    gameObject.AddComponent(type);
                }
            }
        }
    }

    private void Awake() {
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        collider = GetComponent<PolygonCollider2D>();
    }

    private void ApplyCollider() {
        if (collider == null) { return; }

        if (data.UseSpriteCollider && spriteRenderer.sprite != null) {
            // 스프라이트 물리 콜라이더 적용
            var sprite = spriteRenderer.sprite;
            int shapeCount = sprite.GetPhysicsShapeCount();
            collider.pathCount = shapeCount;

            var path = new List<Vector2>();
            for (int i = 0; i < shapeCount; i++) {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                collider.SetPath(i, path);
            }
        }
        else {
            // 기본 패스 (한 칸 꽉 채우기) 적용
            collider.pathCount = 1;

            Vector2[] defaultPath = new Vector2[] {
                new Vector2(0f, -0.5f),
                new Vector2(1f,  0f),
                new Vector2(0f,  0.5f),
                new Vector2(-1f, 0f)
            };
            collider.SetPath(0, defaultPath);
        }
    }

    public void ApplyRotation() {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                int safeIndex = Mathf.Clamp(rotationIndex, 0, data.Sprites.Count - 1);
                spriteRenderer.sprite = data.Sprites[safeIndex];
                spriteRenderer.flipX = false;
            }
            else if (data.UseFlipX) {
                spriteRenderer.sprite = data.Sprites[0];
                spriteRenderer.flipX = (rotationIndex % 2 == 1);
            }
            ApplyCollider();
        }
    }

    public void SetRotationIndex(int index) {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                rotationIndex = (index + data.Sprites.Count) % data.Sprites.Count;
            }
            else if (data.UseFlipX) {
                rotationIndex = (index + 2) % 2;
            }
            ApplyRotation();
        }
    }

    public void RotateNext() {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                rotationIndex = (rotationIndex + 1) % data.Sprites.Count;
            }
            else if (data.UseFlipX) {
                rotationIndex = (rotationIndex + 1) % 2;
            }
            ApplyRotation();
        }
    }

    public void RotatePrev() {
        if (data.IsRotatable) {
            if (!data.UseFlipX && data.Sprites != null && data.Sprites.Count > 0) {
                rotationIndex = (rotationIndex - 1 + data.Sprites.Count) % data.Sprites.Count;
            }
            else if (data.UseFlipX) {
                rotationIndex = (rotationIndex - 1 + 2) % 2;
            }
            ApplyRotation();
        }
    }

    public void PlayAppear() {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AppearRoutine());
    }

    public void PlayDisappear() {
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(Vector3.one, Vector3.zero, () => gameObject.SetActive(false)));
    }

    private IEnumerator AppearRoutine() {
        visual.localPosition = Vector3.zero + new Vector3(0, 0.5f, 0);

        // 일단 커지기
        yield return StartCoroutine(ScaleRoutine(Vector3.zero, Vector3.one));

        yield return new WaitForSecondsRealtime(0.1f);

        // 바운스
        yield return StartCoroutine(MoveRoutine(new Vector3(0, 0.5f, 0), Vector3.zero, 0.2f, easeIn));
        AddDustParticle(8);
        yield return StartCoroutine(MoveRoutine(Vector3.zero, new Vector3(0, 0.3f, 0), 0.15f, easeOut));
        yield return StartCoroutine(MoveRoutine(new Vector3(0, 0.3f, 0), Vector3.zero, 0.15f, easeIn));
        AddDustParticle(5);
        yield return StartCoroutine(MoveRoutine(Vector3.zero, new Vector3(0, 0.15f, 0), 0.1f, easeOut));
        yield return StartCoroutine(MoveRoutine(new Vector3(0, 0.15f, 0), Vector3.zero, 0.1f, easeIn));
        AddDustParticle(3);

        visual.localPosition = Vector3.zero;
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, System.Action onComplete = null) {
        float time = 0f;
        while (time < duration) {
            float t = time / duration;
            float curveT = scaleCurve.Evaluate(t);
            visual.localScale = Vector3.LerpUnclamped(from, to, curveT);
            time += Time.deltaTime;
            yield return null;
        }
        visual.localScale = to;
        onComplete?.Invoke();
    }

    private IEnumerator MoveRoutine(Vector3 from, Vector3 to, float duration, AnimationCurve ease) {
        float time = 0f;
        while (time < duration) {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float easedT = ease.Evaluate(t);
            visual.localPosition = Vector3.Lerp(from, to, easedT);
            yield return null;
        }
        visual.localPosition = to;
    }

    private void AddDustParticle(int num = 5) {
        for (int i = 0; i < num; i++) {
            var particle = (Particle)Random.Range(0, 2);
			var dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			var particleID = EnvironmentManager.AddParticle(particle, transform.position);
			EnvironmentManager.SetParticleVelocity(particleID, dir * 1.5f);
        }
    }
}
