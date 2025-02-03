using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Animal
{
    public string Name{ get; set; }

    public void Eat()
    {
        Debug.Log(Name + " is eating");
    }
}

// C#에서 다중 상속은 안 된다. 콤마 이후에 적는 건 인터페이스
class Dog : Animal // , (인터페이스)
{
    public void Bark()
    {
        Debug.Log(Name + " is barking");
    }
}


// 참조에 관한 예
class A
{
    public A()
    {
        B b = new B(Run);
        b.Run();
    }

    public void Run()
    {
        Debug.Log("Run!!");
    }
}

class B
{
    public delegate void RunDelegate(); // delegate : 대리자
    public RunDelegate AfterRun;

    // 생성자
    public B(RunDelegate afterRun)
    {
        AfterRun = afterRun;
    }
    
    public void Run()
    {
        Debug.Log("Run!!");
        AfterRun.Invoke();
    }
}

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Dog dog = new Dog();
        dog.Name = "Bark";
        dog.Eat();
        dog.Bark();

        // 업캐스팅
        Animal animal = new Dog(); // 도그의 함수 사용 불가능, 애니멀 타입의 변수 도그값 참조
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // 재귀함수
    void RecursiveFunction(int count)
    {
        if (count <= 0)
        {
            Debug.Log("종료");
            return;
        }
        
        Debug.Log("count: " + count);
        RecursiveFunction(count - 1);
    }
}
