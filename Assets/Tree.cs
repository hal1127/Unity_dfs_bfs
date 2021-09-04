using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class Tree : MonoBehaviour
{
  public GameObject node;
  public GameObject arrow;
  public GameObject line;
  public GameObject tree_text;

  public List<int> array;

  List<List<int>> tree_array = new List<List<int>>();
  List<List<GameObject>> tree_go = new List<List<GameObject>>();

  Vector2 node_pos = new Vector2(0, 3);
  Vector2 node_offset = new Vector2(-1.3f, -1.3f);

  bool played = true;

  private IEnumerator DelayCoroutine(float seconds, Action action)
  {
    yield return new WaitForSeconds(seconds);
    action?.Invoke();
  }

  void create_tree_array(List<int> a)
  {
    for (int i = 0; i < a.Count+1; i++) {
      tree_array.Add(new List<int>());
      tree_go.Add(new List<GameObject>());
    }
    tree_go.Add(new List<GameObject>());
    for (int i = 0; i < a.Count; i++) {
      int parent = a[i];
      tree_array[parent].Add(i+1);
    }
  }

  Vector2 create_node(Vector2 parent_pos, int index, int parent_index, int chd_index)
  {
    Vector2 now_pos;
    if (index == 0) {
      now_pos = new Vector2(0, 0);
    } else if (chd_index == 0) {
      now_pos = parent_pos + node_offset;
    } else {
      now_pos = parent_pos + node_offset * new Vector2(-1, 1);
    }

    var node_go = Instantiate(node,
                              now_pos,
                              Quaternion.identity);

    tree_go[parent_index+1].Add(node_go);

    var line_go = Instantiate(line,
                              new Vector2(0, 0),
                              Quaternion.identity);
    var line_rnd = line_go.GetComponent<LineRenderer>();
    line_rnd.SetPosition(0, parent_pos);
    line_rnd.SetPosition(1, now_pos);

    var canvas = node_go.transform.Find("Canvas").gameObject;
    var text_go = canvas.transform.Find("Text").gameObject;
    var text = text_go.GetComponent<Text>();
    text.text = index.ToString();

    return now_pos;
  }

	void create_tree(int index, int parent_index, int chld_index, List<List<int>> tree, Vector2 parent_pos)
	{
    parent_pos = create_node(parent_pos, index, parent_index, chld_index);

    if (tree_array[index].Count == 0) {
      return;
    }
    for (int i = 0; i < tree_array[index].Count; i++) {
      if (chld_index == 1) {
        create_tree(tree_array[index][i], index, 1-i, tree_array, parent_pos);
      } else {
        create_tree(tree_array[index][i], index, i, tree_array, parent_pos);
      }
    }
	}

  // コルーチン。本体の処理と非同期に実行される
  // 深さ優先探索
  IEnumerator dfs(GameObject go, int index)
  {
    yield return new WaitForSeconds(1.0f);
    var circle = go.transform.Find("Circle").gameObject;
    circle.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    // 一秒停止
    yield return new WaitForSeconds(1.0f);
    circle.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    for (int i = 0; i < tree_array[index].Count; i++) {
      // このコルーチンを一旦停止して別のコルーチンを実行
      yield return StartCoroutine(dfs(tree_go[index+1][i], tree_array[index][i]));
    }
  }

  // コルーチン。本体の処理と非同期に実行される
  // 幅優先探索
  IEnumerator bfs()
  {
    Queue<int> q = new Queue<int>();
    q.Enqueue(0);
    while (q.Count != 0) {
      int now = q.Dequeue();
      foreach (var go in tree_go[now]) {
        yield return new WaitForSeconds(1.0f);
        var circle = go.transform.Find("Circle").gameObject;
        circle.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        yield return new WaitForSeconds(1.0f);
        circle.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
      }
      foreach (var t in tree_array[now]) {
        q.Enqueue(t);
      }
    }
  }

  void Start()
  {
    string log = "{ ";
    create_tree_array(array);
    for (int i = 0; i < tree_array.Count; i++) {
      log += i.ToString()+": "+"{ ";
      foreach (var e in tree_array[i]) {
        log += e.ToString() + ", ";
      }
      log += " }, ";
    }
    var text = tree_text.GetComponent<Text>();
    text.text = log + " }";

    Vector2 parent_pos = new Vector2(0, 0);

    create_tree(0, -1, 0, tree_array, parent_pos);
    // for (int i = 0; i < tree_go.Count; i++) {
    //   string cnt_log = (i-1).ToString()+": "+tree_go[i].Count.ToString();
    //   Debug.Log(cnt_log);
    // }
    // dfs(tree_go[0][0], null, 0);
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown("d") && played) {
      played = false;
      Debug.Log("Downed d-key");
      // コルーチンを実行
      StartCoroutine(dfs(tree_go[0][0], 0));
    } else if (Input.GetKeyDown("b") && played) {
      played = false;
      Debug.Log("Downed b-key");
      StartCoroutine(bfs());
    }
  }
}
