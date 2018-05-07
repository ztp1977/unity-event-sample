using System;
using System.Collections;
using JetBrains.Annotations;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UniRxSample : MonoBehaviour
{

	[SerializeField] private Button btnClickMe = null;
	[SerializeField] private Text textTrigger = null;
	
	public bool IsPaused;

	private ReactiveProperty<string> triggerText = new ReactiveProperty<string>();
	
	private void Awake()
	{
		Debug.Log("awake.");
	}

	private void Start()
	{
		// 1. schedule
		timer("start");

		// 2. http downloader
		downloader("http://google.co.jp");
		downloader("http://google_not-found");

		// 3. button onclick
		buttonOnClick();

		// 4. from coroutine
		Debug.Log("start coroutine.");
		Observable.FromCoroutine(NantokaCoroutine).Subscribe(
			_ => { Debug.Log("OnNext!, 成功する処理はこちらで書く!!"); },
			() => { Debug.Log("OnCompleted!"); });
		Debug.LogWarning("end coroutine is fake, 待たないよ!!");

		Observable.FromCoroutine<long>(observer => CountCoroutine(observer)).Subscribe(x => Debug.Log(x)).AddTo(this);

		// 5. reactive property, OK

		// 6. trigger
		triggerText.SubscribeToText(textTrigger);
		{
			// Get the plain object
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

			// Add ObservableXxxTrigger for handle MonoBehaviour's event as Observable
			cube.AddComponent<ObservableUpdateTrigger>()
				.UpdateAsObservable()
				.SampleFrame(30)
				.Subscribe(x => Debug.Log("cube"), () => Debug.Log("destroy"));

			// destroy after 3 second:)
			Destroy(cube, 5f);
		}

		// 7. subscribe and publish

		// 8. error handle

		// 9. thread pool

		// 10. cancelable
	}

	private IEnumerator NantokaCoroutine()
	{
		Debug.Log("Coroutine started.");
		yield return new WaitForSeconds(3.0f);
		Debug.Log("Coroutine finished.");
	}

	private IEnumerator CountCoroutine(IObserver<long> observer)
	{
		long current = 0;
		float deltaTime = 0;

		while (true)
		{
			if (!IsPaused)
			{
				deltaTime += Time.deltaTime;
				if (deltaTime >= 1.0f)
				{
					var integerPart = Mathf.FloorToInt(deltaTime);
					current += integerPart;
					deltaTime -= integerPart;
					
					observer.OnNext(current);
				}
			}
			yield return null;
		}
	}


	private void buttonOnClick()
	{
		btnClickMe.onClick.AsObservable().Subscribe(x =>
		{
			Debug.Log("Button Clicked.");
			triggerText.Value = string.Format("Button clicked, {0}", Time.timeSinceLevelLoad);
		});
	}

	private void downloader([NotNull] string url = "")
	{
		if (url == null) throw new ArgumentNullException("url");
		ObservableWWW.Get(url).Subscribe(
			x =>
			{
				Debug.LogFormat("on next:{0}", x.Substring(0, 100));
			},
			exception =>
			{
				Debug.LogFormat("on failure, url: {0}.", url);
				Debug.LogException(exception);
			}, 
			() => {
				Debug.LogFormat("on Complete, url: {0}.", url);
			});
	}

	private void timer(string msg)
	{
		// 1.0 start
		Observable.Interval(TimeSpan.FromSeconds(1.0f)).Subscribe(
			_ => {
				Debug.LogFormat("time: {0}, msg: {1}", (int)Time.timeSinceLevelLoad,  msg);	
				if (Time.timeSinceLevelLoad > 60)
				{
					
				}
			}).AddTo(this);
	}

}
