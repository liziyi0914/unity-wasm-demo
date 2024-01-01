pub fn fib(n: u64) -> u64 {
    return match n {
        0 | 1 => n,
        _ => fib(n - 1) + fib(n - 2),
    };
}
